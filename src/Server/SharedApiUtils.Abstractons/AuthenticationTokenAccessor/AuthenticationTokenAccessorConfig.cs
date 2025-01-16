using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharedApiUtils.Abstractons.AuthenticationTokenAccessor;

public static class AuthenticationTokenAccessorConfig
{
    public static IServiceCollection AddAuthenticationTokenAccessor(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuthenticationTokenAccessor>();
        services.AddSingleton(new TokenAccessorConfiguration()
        {
            IgnoreSslVerification = bool.Parse(configuration["PrivateAccessToken:IgnoreSslVerification"]!),
            AuthenticationUrl = configuration["PrivateAccessToken:AuthenticationUrl"]!,
            ClientSecret = configuration["PrivateAccessToken:ClientSecret"]!,
            ClientId = configuration["PrivateAccessToken:ClientId"]!,
            TokenClaim = configuration["PrivateAccessToken:TokenClaim"]!
        });
        return services;
    }
}
