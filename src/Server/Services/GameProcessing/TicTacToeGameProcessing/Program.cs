using Microsoft.AspNetCore.Authentication.JwtBearer;
using SharedApiUtils.RabbitMq;
using System.Security.Claims;
using TicTacToeGameProcessing.Interfaces;
using TicTacToeGameProcessing.Services;

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
builder.Services.AddGrpc();
builder.Services.AddScoped<ITicTacToeGameProcessingService, TicTacToeGameProcessingService>();
builder.Services.AddSingleton<Random>();
builder.Services.AddSingleton(new RabbitMqConfiguration()
{
    Host = builder.Configuration["RabbitMqConfiguration:Host"]!,
    Username = builder.Configuration["RabbitMqConfiguration:Username"]!,
    Password = builder.Configuration["RabbitMqConfiguration:Password"]!
});
builder.Services.AddSingleton<RabbitMqConnection>();
builder.Services.AddSingleton<TicTacToeGameProccessingRabbitMqService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roomsEventListener = services.GetRequiredService<TicTacToeGameProccessingRabbitMqService>();
    await roomsEventListener.StartListening("tictactoe");
}
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<TicTacToeGameProcessingRpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
