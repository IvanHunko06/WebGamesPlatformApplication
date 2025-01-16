using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedApiUtils.RabbitMq;

public static class RabbitMqConfig
{
    public static IServiceCollection AddCustomRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new RabbitMqConfiguration()
        {
            Host = configuration["RabbitMqConfiguration:Host"]!,
            Username = configuration["RabbitMqConfiguration:Username"]!,
            Password = configuration["RabbitMqConfiguration:Password"]!
        });
        services.AddSingleton<RabbitMqConnection>();
        return services;
    }
}
