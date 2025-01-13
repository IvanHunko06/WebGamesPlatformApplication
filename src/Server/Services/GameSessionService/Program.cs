using GameSessionService;
using GameSessionService.Interfaces;
using GameSessionService.Repositories;
using GameSessionService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Clients;
using System.Security.Claims;

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
builder.Services.AddScoped<IRoomsServiceClient, RabbitMqRoomsServiceClient>();
builder.Services.AddScoped<ISessionsRepository, RedisSessionsRepository>();
builder.Services.AddScoped<IGameProcessingServiceClient, RabbitMqGameProccessingClient>();
builder.Services.AddScoped<GameProcessingServiceConnection>();
builder.Services.AddScoped<RedisHelper>();
builder.Services.AddGrpc();
builder.Services.AddSingleton(new RabbitMqConfiguration()
{
    Host = builder.Configuration["RabbitMqConfiguration:Host"]!,
    Username = builder.Configuration["RabbitMqConfiguration:Username"]!,
    Password = builder.Configuration["RabbitMqConfiguration:Password"]!
});
builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddScoped<IGameSessionService, GameSessionService.Services.GameSessionService>();
builder.Services.AddSingleton<GameSessionRabbitMqService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roomsEventListener = services.GetRequiredService<GameSessionRabbitMqService>();
    await roomsEventListener.StartListening();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGrpcService<GameSessionService.Services.GameSessionRpcService>();

app.Run();
