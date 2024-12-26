

using Microsoft.AspNetCore.Authentication.JwtBearer;
using RoomsService;
using RoomsService.Repositories;
using RoomsService.Services;
using SharedApiUtils.Interfaces;
using SharedApiUtils.ServicesAccessing;
using SharedApiUtils.ServicesAccessing.Connections;
using SharedApiUtils.ServicesClients;
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
    IgnoreSslVerification = accessTokenSection.GetValue<bool>("IgnoreSslVerification"),
    AuthenticationUrl = accessTokenSection.GetValue<string>("AuthenticationUrl") ?? throw new ArgumentNullException("authentication url is null"),
    ClientSecret = accessTokenSection.GetValue<string>("ClientSecret") ?? throw new ArgumentNullException("client secret is null"),
    ClientId = accessTokenSection.GetValue<string>("ClientId") ?? throw new ArgumentNullException("client id is null"),
    TokenClaim = accessTokenSection.GetValue<string>("TokenClaim") ?? throw new ArgumentNullException("token claim is null")
});
builder.Services.AddScoped<GamesServiceConnection>();
builder.Services.AddScoped<RoomsEventsConnection>();
var gamesServiceSection = builder.Configuration.GetRequiredSection("ExternalServices");
builder.Services.AddSingleton(new AccessingConfiguration()
{
    IgnoreSslVerification = gamesServiceSection.GetValue<bool>("IgnoreSslVerification"),
    GamesServiceUrl = gamesServiceSection.GetValue<string>("GamesService") ?? throw new ArgumentNullException("Games service url is null"),
    WebSocketServiceUrl = gamesServiceSection.GetValue<string>("WebSocketService") ?? throw new ArgumentNullException("Web socket service url is null"),
    RoomsEventsHandlerUrl = gamesServiceSection.GetValue<string>("WebSocketService") ?? throw new ArgumentNullException("Web socket service url is null")
});
builder.Services.AddScoped<RedisRoomRepository>();
builder.Services.AddScoped<IGamesServiceClient, GamesServiceClient>();
builder.Services.AddScoped<RoomEventNotifier>();
builder.Services.AddScoped<RoomValidationService>();
builder.Services.AddScoped<UserContextService>();
builder.Services.AddHostedService<RoomCleanupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGrpcService<RoomsService.Services.RoomsService>();
app.Run();
