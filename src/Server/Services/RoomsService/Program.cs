using RoomsService;
using RoomsService.Interfaces;
using RoomsService.Repositories;
using RoomsService.Services;
using SharedApiUtils.Abstractons.Authentication;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.gRPC.ServicesClients;
using SharedApiUtils.RabbitMq;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddScoped<RedisHelper>();
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
builder.Services.AddControllers();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roomsEventListener = services.GetRequiredService<RoomsRabbitMqService>();
    await roomsEventListener.StartListening();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<RoomsService.Services.RoomsRpcService>();
app.Run();
