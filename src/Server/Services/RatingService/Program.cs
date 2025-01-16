using Microsoft.EntityFrameworkCore;
using RatingService;
using RatingService.Interfaces;
using RatingService.Repositories;
using RatingService.Services;
using SharedApiUtils.Abstractons.Authentication;
using SharedApiUtils.RabbitMq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddGrpc();
var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<RatingDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddHostedService<SeasonAddService>();
builder.Services.AddScoped<IRatingRepository, SqlServerRatingRepository>();
builder.Services.AddScoped<IRatingService, RatingService.Services.RatingService>();
builder.Services.AddCustomRabbitMq(builder.Configuration);
builder.Services.AddSingleton<RatingRabbitMqService>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<RatingDbContext>();
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        await context.Database.MigrateAsync();
    }
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var ratingService = services.GetRequiredService<RatingRabbitMqService>();
    await ratingService.StartListening();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapGrpcService<RatingService.Services.RatingRpcService>();
app.MapControllers();

app.Run();
