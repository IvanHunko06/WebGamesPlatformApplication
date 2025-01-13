

using Microsoft.AspNetCore.Authentication.JwtBearer;
using RoomsService;
using RoomsService.Interfaces;
using RoomsService.Repositories;
using RoomsService.Services;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.gRPC.ServicesClients;
using SharedApiUtils.RabbitMq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);
var auth = builder.Configuration.GetRequiredSection("Authentication");
var validIssuers = auth.GetSection("ValidIssuers").Get<IEnumerable<string>>() ?? throw new ArgumentException("Valid issuers not declared");
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
builder.Services.AddScoped<RedisHelper>();
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
builder.Services.AddGrpc();

builder.Services.AddScoped<AuthenticationTokenAccessor>();
var accessTokenSection = builder.Configuration.GetRequiredSection("PrivateAccessToken");

builder.Services.AddSingleton(new TokenAccessorConfiguration()
{
    IgnoreSslVerification = bool.Parse(builder.Configuration["PrivateAccessToken:IgnoreSslVerification"]!),
    AuthenticationUrl = builder.Configuration["PrivateAccessToken:AuthenticationUrl"]!,
    ClientSecret = builder.Configuration["PrivateAccessToken:ClientSecret"]!,
    ClientId = builder.Configuration["PrivateAccessToken:ClientId"]!,
    TokenClaim = builder.Configuration["PrivateAccessToken:TokenClaim"]!
});
builder.Services.AddScoped<GamesServiceConnection>();
builder.Services.AddScoped<RoomsEventsConnection>();
builder.Services.AddSingleton(new AccessingConfiguration()
{
    IgnoreSslVerification = bool.Parse(builder.Configuration["ExternalServices:IgnoreSslVerification"]!),
    GamesServiceUrl = builder.Configuration["ExternalServices:GamesService"]!,
    WebSocketServiceUrl = builder.Configuration["ExternalServices:WebSocketService"]!,
    RoomsEventsHandlerUrl = builder.Configuration["ExternalServices:WebSocketService"]!
});
builder.Services.AddScoped<IRoomRepository, RedisRoomRepository>();
builder.Services.AddScoped<ICacheRepository, RedisCacheRepository>();
builder.Services.AddScoped<IRoomsService, RoomsService.Services.RoomsService>();
builder.Services.AddScoped<IGamesServiceClient, RPCGamesServiceClient>();
builder.Services.AddScoped<IRoomEventNotifier, RabbitmqRoomEventNotifier>();
builder.Services.AddScoped<RoomValidationService>();
builder.Services.AddScoped<UserContextService>();
builder.Services.AddHostedService<RoomCleanupService>();
builder.Services.AddSingleton(new RabbitMqConfiguration()
{
    Host = builder.Configuration["RabbitMqConfiguration:Host"]!,
    Username = builder.Configuration["RabbitMqConfiguration:Username"]!,
    Password = builder.Configuration["RabbitMqConfiguration:Password"]!
});
builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddSingleton<RoomsRabbitMqService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roomsEventListener = services.GetRequiredService<RoomsRabbitMqService>();
    await roomsEventListener.StartListening();
}
app.MapGrpcService<RoomsService.Services.RoomsRpcService>();
app.Run();
