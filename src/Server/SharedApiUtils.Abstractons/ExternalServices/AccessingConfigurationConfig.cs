using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedApiUtils.Abstractons.ExternalServices;

public static class AccessingConfigurationConfig
{
    public static IServiceCollection AddAccessingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new AccessingConfiguration()
        {
            IgnoreSslVerification = bool.Parse(configuration["ExternalServices:IgnoreSslVerification"]!),
            GamesServiceUrl = configuration["ExternalServices:GamesService"],
            RoomsServiceUrl = configuration["ExternalServices:RoomsService"],
            GameSessionServiceUrl = configuration["ExternalServices:GameSessionService"],
            MatchHistoryServiceUrl = configuration["ExternalServices:MatchHistoryService"],
            RatingServiceUrl = configuration["ExternalServices:RatingService"],
            WebSocketServiceUrl = configuration["ExternalServices:WebSocketService"],
            RoomsEventsHandlerUrl = configuration["ExternalServices:RoomsEventsHandler"],
            ProfileServiceUrl = configuration["ExternalServices:ProfileService"]
        });
        return services;
    }
}
