using Microsoft.AspNetCore.Authentication.JwtBearer;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Clients;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using WebSocketService;
using WebSocketService.Hubs;
using WebSocketService.HubStates;
using WebSocketService.Interfaces;
using WebSocketService.Repositories;
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
builder.Services.AddSingleton(new TokenAccessorConfiguration()
{
    IgnoreSslVerification = bool.Parse(builder.Configuration["PrivateAccessToken:IgnoreSslVerification"]!),
    AuthenticationUrl = builder.Configuration["PrivateAccessToken:AuthenticationUrl"]!,
    ClientSecret = builder.Configuration["PrivateAccessToken:ClientSecret"]!,
    ClientId = builder.Configuration["PrivateAccessToken:ClientId"]!,
    TokenClaim = builder.Configuration["PrivateAccessToken:TokenClaim"]!
});
builder.Services.AddScoped<GamesServiceConnection>();
builder.Services.AddScoped<RoomsServiceConnection>();
builder.Services.AddScoped<GameSessionConnection>();
builder.Services.AddSingleton(new AccessingConfiguration()
{
    IgnoreSslVerification = bool.Parse(builder.Configuration["ExternalServices:IgnoreSslVerification"]!),
    GamesServiceUrl = builder.Configuration["ExternalServices:GamesService"]!,
    RoomsServiceUrl = builder.Configuration["ExternalServices:RoomsService"]!,
    GameSessionServiceUrl = builder.Configuration["ExternalServices:GameSessionService"]!
});
builder.Services.AddSingleton<GameIdsList>();
builder.Services.AddScoped<RedisHelper>();
builder.Services.AddSingleton<UserConnectionStateService>();
builder.Services.AddSingleton<IRoomSessionHandlerService, RoomSessionHandlerService>();
builder.Services.AddSingleton<SessionManagmentHubState>();
builder.Services.AddScoped<IUserContextService, UserContextService>();
builder.Services.AddScoped<IRoomsServiceClient, RabbitMqRoomsServiceClient>();
builder.Services.AddScoped<IGameSessionServiceClient, RabbitMqGameSessionClient>();
builder.Services.AddScoped<IServiceInternalRepository, RedisServiceInternalRepository>();
builder.Services.AddScoped<IGameSessionHandlerService, GameSessionHandlerService>();
builder.Services.AddScoped<IRoomSessionHandlerService, RoomSessionHandlerService>();
builder.Services.AddScoped<IRoomsEventsService, RoomsEventsService>();
builder.Services.AddSingleton<RoomsEventsRabbitMqListener>();
builder.Services.AddSingleton(new RabbitMqConfiguration()
{
    Host = builder.Configuration["RabbitMqConfiguration:Host"]!,
    Username = builder.Configuration["RabbitMqConfiguration:Username"]!,
    Password = builder.Configuration["RabbitMqConfiguration:Password"]!
});
builder.Services.AddSingleton<RabbitMqConnection>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roomsEventListener = services.GetRequiredService<RoomsEventsRabbitMqListener>();
    await roomsEventListener.StartListening();
}
app.UseCors("AllowApiGateway");
app.UseMiddleware<JwtFromUriMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<RoomsEventsRpcListener>();
app.MapHub<RoomsHub>("/hubs/rooms-hub");
app.MapHub<SessionManagmentHub>("/hubs/session-managment-hub");
app.Run();
