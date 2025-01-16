using GameSessionService;
using GameSessionService.Interfaces;
using GameSessionService.Repositories;
using GameSessionService.Services;
using SharedApiUtils.Abstractons.Authentication;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Clients;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddScoped<IRoomsServiceClient, RabbitMqRoomsServiceClient>();
builder.Services.AddScoped<ISessionsRepository, RedisSessionsRepository>();
builder.Services.AddScoped<IGameProcessingServiceClient, RabbitMqGameProccessingClient>();
builder.Services.AddSingleton<IGameSessionWsNotifyerClient, RabbitMqGameSessionWsNotifyerClient>();
builder.Services.AddScoped<IRatingServiceClient, RabbitMqRatingServiceClient>();
builder.Services.AddScoped<IMatchHistoryServiceClient, RabbitMqMatchHistoryClient>();
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
builder.Services.AddSingleton<RabbitMqRoomsEventsListener>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var gameSessionListener = services.GetRequiredService<GameSessionRabbitMqService>();
    var roomEventsListener = services.GetRequiredService<RabbitMqRoomsEventsListener>();
    await gameSessionListener.StartListening();
    await roomEventsListener.StartListening();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
app.MapGrpcService<GameSessionService.Services.GameSessionRpcService>();

app.Run();
