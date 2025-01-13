using MatchHistoryService;
using MatchHistoryService.Interfaces;
using MatchHistoryService.Repositories;
using Microsoft.EntityFrameworkCore;
using SharedApiUtils.Abstractons.Authentication;

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
builder.Services.AddControllers();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MatchHistoryDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGrpcService<MatchHistoryService.Services.MatchHistoryRpcService>();
app.Run();
