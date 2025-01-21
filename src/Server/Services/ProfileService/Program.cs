using Microsoft.EntityFrameworkCore;
using ProfileService;
using ProfileService.Interfaces;
using ProfileService.Repositories;
using ProfileService.Services;
using SharedApiUtils.Abstractons.Authentication;
using SharedApiUtils.Abstractons.AuthenticationTokenAccessor;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.RabbitMq;
using SharedApiUtils.RabbitMq.Clients;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCustomAuthentication(builder.Configuration);
builder.Services.AddCustomRabbitMq(builder.Configuration);
builder.Services.AddScoped<KeycloakAdmimClient>();
builder.Services.AddScoped<KeycloakEventsParser>();
var connectionString = builder.Configuration.GetConnectionString("Database");
builder.Services.AddDbContext<ProfileServiceDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddHostedService<KeycloakEventsBackgroundListener>();
builder.Services.AddScoped<IProfilesRepository, SqlServerProfilesRepository>();
builder.Services.AddScoped<IProfileIconsRepository, SqlServerProfileIconsRepository>();
builder.Services.AddScoped<IRatingServiceClient, RabbitMqRatingServiceClient>();
builder.Services.AddScoped<IProfileService, ProfileService.Services.ProfileService>();
builder.Services.AddScoped<UsernameUserContextService>();
builder.Services.AddSingleton(
    new AuthenticationTokenAccessorBuilder()
        .SetAdminTokenInfo(builder.Configuration.GetRequiredSection("AdminAccessToken"))
        .Build()
);
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ProfileServiceDbContext>();
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Any())
    {
        await context.Database.MigrateAsync();
    }
}
app.UseCors("AllowApiGateway");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
