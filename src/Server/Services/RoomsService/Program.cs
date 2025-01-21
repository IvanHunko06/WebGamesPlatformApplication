using RoomsService;
using RoomsService.Interfaces;
using RoomsService.Repositories;
using RoomsService.Services;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Authentication;
using SharedApiUtils.Abstractons.AuthenticationTokenAccessor;
using SharedApiUtils.Abstractons.ExternalServices;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.gRPC.ServicesClients;
using SharedApiUtils.RabbitMq;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddScoped<RedisHelper>();
builder.Services.AddGrpc();

builder.Services.AddSingleton(
    new AuthenticationTokenAccessorBuilder()
        .SetPrivateTokenInfo(builder.Configuration.GetRequiredSection("PrivateAccessToken"))
        .Build()
);
builder.Services.AddScoped<GamesServiceConnection>();
builder.Services.AddScoped<RoomsEventsConnection>();
builder.Services.AddAccessingConfiguration(builder.Configuration);
builder.Services.AddScoped<IRoomRepository, RedisRoomRepository>();
builder.Services.AddScoped<ICacheRepository, RedisCacheRepository>();
builder.Services.AddScoped<IRoomsService, RoomsService.Services.RoomsService>();
builder.Services.AddScoped<IGamesServiceClient, RPCGamesServiceClient>();
builder.Services.AddSingleton<IRoomEventNotifier, RabbitmqRoomEventNotifier>();
builder.Services.AddScoped<RoomValidationService>();
builder.Services.AddScoped<UserContextService>();
builder.Services.AddHostedService<RoomCleanupService>();
builder.Services.AddCustomRabbitMq(builder.Configuration);
builder.Services.AddSingleton<RoomsRabbitMqService>();
builder.Services.AddControllers();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roomsEventListener = services.GetRequiredService<RoomsRabbitMqService>();
    await roomsEventListener.StartListening();
}
app.UseCors("AllowApiGateway");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<RoomsService.Services.RoomsRpcService>();
app.Run();
