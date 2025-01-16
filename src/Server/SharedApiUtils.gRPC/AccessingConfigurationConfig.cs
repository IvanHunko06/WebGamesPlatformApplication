using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedApiUtils.gRPC.ServicesAccessing;

namespace SharedApiUtils.gRPC;

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
            RoomsEventsHandlerUrl = configuration["ExternalServices:RoomsEventsHandler"]
        });
        return services;
    }
}
