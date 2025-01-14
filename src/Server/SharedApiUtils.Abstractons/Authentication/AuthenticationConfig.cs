using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SharedApiUtils.Abstractons.Authentication;

public static class AuthenticationConfig
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AuthSettings>(configuration.GetRequiredSection("Authentication"));
        var serviceProvider = services.BuildServiceProvider();
        var authSettings = serviceProvider.GetRequiredService<IOptions<AuthSettings>>().Value;
        services.AddAuthentication("PrivateClientScheme")
            .AddJwtBearer("PrivateClientScheme", options =>
            {
                options.RequireHttpsMetadata = authSettings.HttpsMetadata;
                options.Audience = authSettings.PrivateAudience;
                options.MetadataAddress = authSettings.MetadataAddress;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidIssuers = authSettings.ValidIssuers,
                };
                if (authSettings.IgnoreSslCertificateValidation)
                {
                    options.BackchannelHttpHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                }
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                        if (claimsIdentity is null) return Task.CompletedTask;
                        claimsIdentity.AddClaim(new Claim("AuthenticationScheme", "PrivateClientScheme"));
                        return Task.CompletedTask;
                    }
                };

            })
            .AddJwtBearer("PublicClientScheme", options =>
            {
                options.RequireHttpsMetadata = authSettings.HttpsMetadata;
                options.Audience = authSettings.PublicAudience;
                options.MetadataAddress = authSettings.MetadataAddress;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidIssuers = authSettings.ValidIssuers,
                };
                if (authSettings.IgnoreSslCertificateValidation)
                {
                    options.BackchannelHttpHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                }
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                        if (claimsIdentity is null) return Task.CompletedTask;
                        claimsIdentity.AddClaim(new Claim("AuthenticationScheme", "PublicClientScheme"));

                        var realmAccessClaim = claimsIdentity.FindFirst("realm_access");
                        if (realmAccessClaim is null) return Task.CompletedTask;

                        var realmAccess = JsonNode.Parse(realmAccessClaim.Value);
                        if (realmAccess is null) return Task.CompletedTask;

                        var roles = realmAccess["roles"]?.AsArray();
                        if (roles is null) return Task.CompletedTask;
                        foreach (var role in roles)
                        {
                            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Deserialize<string>()));
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization(options =>
        {

            options.AddPolicy("AdminOrPrivateClient", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AuthenticationSchemes.Add("PublicClientScheme");
                policy.AuthenticationSchemes.Add("PrivateClientScheme");
                policy.RequireAssertion((context) =>
                {
                    var scheme = context.User.Claims.FirstOrDefault(c => c.Type == "AuthenticationScheme");
                    if (scheme is null)
                        return false;

                    if (scheme.Value == "PublicClientScheme" && context.User.IsInRole(authSettings.AdminRoleClaim))
                        return true;
                    else if (scheme.Value == "PrivateClientScheme")
                        return true;
                    else
                        return false;
                });
            });
            options.AddPolicy("AllAuthenticatedUsers", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AuthenticationSchemes.Add("PublicClientScheme");
                policy.AuthenticationSchemes.Add("PrivateClientScheme");
            });
            options.AddPolicy("OnlyPublicClient", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AuthenticationSchemes.Add("PublicClientScheme");
            });
            options.AddPolicy("OnlyPrivateClient", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AuthenticationSchemes.Add("PrivateClientScheme");
            });
        });
        return services;
    }
}
