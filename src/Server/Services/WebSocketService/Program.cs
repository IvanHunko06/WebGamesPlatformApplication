using Microsoft.AspNetCore.Authentication.JwtBearer;
using SharedApiUtils.ServicesAccessing;
using SharedApiUtils.ServicesAccessing.Connections;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebSocketService;
using WebSocketService.Hubs;
using WebSocketService.HubStates;
using WebSocketService.Services;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddGrpc();
var auth = builder.Configuration.GetRequiredSection("Authentication");
var validIssuers = auth.GetRequiredSection("ValidIssuers").Get<IEnumerable<string>>() ?? throw new ArgumentException("Valid issuers not declared");
string metadataAddress = auth.GetValue<string>("MetadataAddress") ?? throw new ArgumentException("Metadata address is null");
bool requireHttpsMetadata = auth.GetValue<bool>("HttpsMetadata");
string adminRoleClaim = auth.GetValue<string>("AdminRoleClaim") ?? throw new ArgumentException("Admin role claim is null");
bool ignoreSslValidation = auth.GetValue<bool>("IgnoreSslCertificateValidation");
builder.Services.AddAuthentication("PrivateClientScheme")
    .AddJwtBearer("PrivateClientScheme", options =>
    {
        options.RequireHttpsMetadata = requireHttpsMetadata;
        options.Audience = auth.GetValue<string>("PrivateAudience") ?? throw new ArgumentException("Private client audience is null");
        options.MetadataAddress = metadataAddress;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuers = validIssuers,
        };
        if (ignoreSslValidation)
        {
            options.BackchannelHttpHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
        }
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity is null) return Task.CompletedTask;
                claimsIdentity.AddClaim(new Claim("AuthenticationScheme", "PrivateClientScheme"));
                return Task.CompletedTask;
            }
        };

    })
    .AddJwtBearer("PublicClientScheme", options =>
    {
        options.RequireHttpsMetadata = requireHttpsMetadata;
        options.Audience = auth.GetValue<string>("PublicAudience") ?? throw new ArgumentException("Public client audience is null");
        options.MetadataAddress = metadataAddress;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidIssuers = validIssuers,
        };
        if (ignoreSslValidation)
        {
            options.BackchannelHttpHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
        }
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                if (claimsIdentity is null) return Task.CompletedTask;
                claimsIdentity.AddClaim(new Claim("AuthenticationScheme", "PublicClientScheme"));

                var realmAccessClaim = claimsIdentity.FindFirst("realm_access");
                if (realmAccessClaim is null) return Task.CompletedTask;

                var realmAccess = JsonNode.Parse(realmAccessClaim.Value);
                if (realmAccess is null) return Task.CompletedTask;

                var roles = realmAccess["roles"]?.AsArray();
                if (roles is null) return Task.CompletedTask;
                foreach (var role in roles)
                {
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Deserialize<string>()));
                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{

    options.AddPolicy("AdminOrPrivateClient", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AuthenticationSchemes.Add("PublicClientScheme");
        policy.AuthenticationSchemes.Add("PrivateClientScheme");
        policy.RequireAssertion((context) =>
        {
            var scheme = context.User.Claims.FirstOrDefault(c => c.Type == "AuthenticationScheme");
            if (scheme is null)
                return false;

            if (scheme.Value == "PublicClientScheme" && context.User.IsInRole(adminRoleClaim))
                return true;
            else if (scheme.Value == "PrivateClientScheme")
                return true;
            else
                return false;
        });
    });
    options.AddPolicy("AllAuthenticatedUsers", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AuthenticationSchemes.Add("PublicClientScheme");
        policy.AuthenticationSchemes.Add("PrivateClientScheme");
    });
    options.AddPolicy("OnlyPublicClient", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AuthenticationSchemes.Add("PublicClientScheme");
    });
    options.AddPolicy("OnlyPrivateClient", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AuthenticationSchemes.Add("PrivateClientScheme");
    });
});
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
var configuration = builder.Configuration;
string[] corsDomains = configuration.GetRequiredSection("CorsDomains").Get<string[]>() ?? throw new ArgumentNullException("Cors domains is null");
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApiGateway", builder =>
    builder
    .WithOrigins(corsDomains)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());
});

builder.Services.AddScoped<AuthenticationTokenAccessor>();
var accessTokenSection = builder.Configuration.GetRequiredSection("PrivateAccessToken");
builder.Services.AddSingleton(new TokenAccessorConfiguration()
{
    IgnoreSslVerification = accessTokenSection.GetValue<bool>("IgnoreSslVerification"),
    AuthenticationUrl = accessTokenSection.GetValue<string>("AuthenticationUrl") ?? throw new ArgumentNullException("authentication url is null"),
    ClientSecret = accessTokenSection.GetValue<string>("ClientSecret") ?? throw new ArgumentNullException("client secret is null"),
    ClientId = accessTokenSection.GetValue<string>("ClientId") ?? throw new ArgumentNullException("client id is null"),
    TokenClaim = accessTokenSection.GetValue<string>("TokenClaim") ?? throw new ArgumentNullException("token claim is null")
});
builder.Services.AddScoped<GamesServiceConnection>();
builder.Services.AddScoped<RoomsServiceConnection>();
var externalServicesSection = builder.Configuration.GetRequiredSection("ExternalServices");
builder.Services.AddSingleton(new AccessingConfiguration()
{
    IgnoreSslVerification = externalServicesSection.GetValue<bool>("IgnoreSslVerification"),
    GamesServiceUrl = externalServicesSection.GetValue<string>("GamesService") ?? throw new ArgumentNullException("Games service url is null"),
    RoomsServiceUrl = externalServicesSection.GetValue<string>("RoomsService") ?? throw new ArgumentNullException("Rooms service url is null"),
});
builder.Services.AddSingleton<GameIdsList>();
builder.Services.AddSingleton<UserConnectionsList>();
builder.Services.AddScoped<RedisHelper>();
builder.Services.AddSingleton<UserConnectionStateService>();
builder.Services.AddSingleton<RoomSessionHandlerService>();
builder.Services.AddSingleton<SessionManagmentHubState>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var redisHelper = services.GetRequiredService<RedisHelper>();
    var database = redisHelper.GetRedisDatabase();
    var server = redisHelper.GetRedisServer();
    await foreach (var key in server.KeysAsync(pattern: "*UserConnections*"))
    {
        await database.KeyDeleteAsync(key);
    }
}
app.UseCors("AllowApiGateway");
app.UseMiddleware<JwtFromUriMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<RoomsEventsService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
//app.MapHub<MainHub>("/hubs/main-hub");
app.MapHub<RoomsHub>("/hubs/rooms-hub");
app.MapHub<SessionManagmentHub>("/hubs/session-managment-hub");
app.Run();
