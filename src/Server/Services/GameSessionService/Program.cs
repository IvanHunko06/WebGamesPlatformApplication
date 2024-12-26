using GameSessionService;
using GameSessionService.Interface;
using GameSessionService.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyPrivateClient", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AuthenticationSchemes.Add("PrivateClientScheme");
    });
});
builder.Services.AddScoped<IGamesServiceClient, GamesServiceClient>();
builder.Services.AddScoped<IRoomsServiceClient, RoomsServiceClient>();
builder.Services.AddScoped<RoomsServiceConnection>();
builder.Services.AddScoped<GamesServiceConnection>();
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
var gamesServiceSection = builder.Configuration.GetRequiredSection("ExternalServices");
builder.Services.AddSingleton(new AccessingConfiguration()
{
    IgnoreSslVerification = gamesServiceSection.GetValue<bool>("IgnoreSslVerification"),
    GamesServiceUrl = gamesServiceSection.GetValue<string>("GamesService") ?? throw new ArgumentNullException("Games service url is null"),
    RoomsServiceUrl = gamesServiceSection.GetValue<string>("RoomsService") ?? throw new ArgumentNullException("Rooms service url is null"),
});
builder.Services.AddScoped<ISessionsRepository, RedisSessionsRepository>();
builder.Services.AddScoped<IGameProcessingServiceClient, GameProcessingServiceClient>();
builder.Services.AddScoped<GameProcessingServiceConnection>();
builder.Services.AddScoped<RedisHelper>();
builder.Services.AddGrpc();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGrpcService<GameSessionService.Services.GameSessionService>();

app.Run();
