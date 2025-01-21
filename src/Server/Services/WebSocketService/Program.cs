using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Authentication;
using SharedApiUtils.Abstractons.AuthenticationTokenAccessor;
using SharedApiUtils.Abstractons.ExternalServices;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Clients;
using WebSocketService;
using WebSocketService.Hubs;
using WebSocketService.HubStates;
using WebSocketService.Interfaces;
using WebSocketService.Repositories;
using WebSocketService.Services;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
var configuration = builder.Configuration;

builder.Services.AddSingleton(
    new AuthenticationTokenAccessorBuilder()
        .SetPrivateTokenInfo(builder.Configuration.GetRequiredSection("PrivateAccessToken"))
        .Build()
);
builder.Services.AddScoped<GamesServiceConnection>();
builder.Services.AddScoped<RoomsServiceConnection>();
builder.Services.AddScoped<GameSessionConnection>();
builder.Services.AddAccessingConfiguration(configuration);
builder.Services.AddSingleton<GameIdsList>();
builder.Services.AddScoped<RedisHelper>();
builder.Services.AddSingleton<UserConnectionStateService>();
builder.Services.AddSingleton<IRoomSessionHandlerService, RoomSessionHandlerService>();
builder.Services.AddSingleton<SessionManagmentHubState>();
builder.Services.AddScoped<UserContextService>();
builder.Services.AddScoped<IRoomsServiceClient, RabbitMqRoomsServiceClient>();
builder.Services.AddScoped<IGameSessionServiceClient, RabbitMqGameSessionClient>();
builder.Services.AddScoped<IServiceInternalRepository, RedisServiceInternalRepository>();
builder.Services.AddScoped<IGameSessionHandlerService, GameSessionHandlerService>();
builder.Services.AddScoped<IRoomSessionHandlerService, RoomSessionHandlerService>();
builder.Services.AddScoped<IRoomsEventsService, RoomsEventsService>();
builder.Services.AddScoped<IGameSessionWsNotifyer, GameSessionWsNotifyer>();
builder.Services.AddSingleton<RoomsEventsRabbitMqListener>();
builder.Services.AddSingleton<RabbitMqGameSessionWsNotifyer>();
builder.Services.AddCustomRabbitMq(builder.Configuration);
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roomsEventListener = services.GetRequiredService<RoomsEventsRabbitMqListener>();
    var gameSessionWsNotifyer = services.GetRequiredService<RabbitMqGameSessionWsNotifyer>();
    await roomsEventListener.StartListening();
    await gameSessionWsNotifyer.StartListening();
}
app.UseCors("AllowApiGateway");
app.UseMiddleware<JwtFromUriMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<RoomsEventsRpcListener>();
app.MapHub<RoomsHub>("/hubs/rooms-hub");
app.MapHub<SessionManagmentHub>("/hubs/session-managment-hub");
app.Run();
