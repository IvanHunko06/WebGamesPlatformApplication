//using GamesService.Services;

using GamesService;
using GamesService.Interfaces;
using GamesService.Repositories;
using Microsoft.EntityFrameworkCore;
using SharedApiUtils.Abstractons.Authentication;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddGrpc();
builder.Services.AddDbContext<GamesService.GamesServerDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});
builder.Services.AddScoped<IGamesRepository, SqlServerGamesRepository>();
builder.Services.AddScoped<IGamesService, GamesService.Services.GamesService>();
builder.Services.AddControllers();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<GamesServerDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GamesService.Services.GamesRpcService>();
app.Run();
