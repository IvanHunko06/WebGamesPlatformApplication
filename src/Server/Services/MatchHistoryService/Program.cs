using MatchHistoryService;
using MatchHistoryService.Interfaces;
using MatchHistoryService.Repositories;
using MatchHistoryService.Services;
using Microsoft.EntityFrameworkCore;
using SharedApiUtils.Abstractons.Authentication;
using SharedApiUtils.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddGrpc();
var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<MatchHistoryDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});

builder.Services.AddScoped<IMatchInfoRepository, SqlServerMatchInfoRepository>();
builder.Services.AddScoped<IMatchHistoryService, MatchHistoryService.Services.MatchHistoryService>();
builder.Services.AddCustomRabbitMq(builder.Configuration);
builder.Services.AddSingleton<MatchHistoryRabbitMqService>();
builder.Services.AddControllers();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MatchHistoryDbContext>();
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        await context.Database.MigrateAsync();
    }
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var matchHistoryService = services.GetRequiredService<MatchHistoryRabbitMqService>();
    await matchHistoryService.StartListening();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<MatchHistoryService.Services.MatchHistoryRpcService>();
app.Run();
