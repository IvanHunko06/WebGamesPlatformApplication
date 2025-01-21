using Microsoft.Extensions.Configuration;

namespace SharedApiUtils.Abstractons.AuthenticationTokenAccessor;

public class AuthenticationTokenAccessorBuilder : IAuthenticationTokenAccessorBuilder
{
    AdminTokenConfiguration? adminToken;
    PrivateTokenConfiguration? privateToken;
    public IAuthenticationTokenAccessorBuilder SetPrivateTokenInfo(IConfigurationSection privateTokenSection)
    {
        privateToken = new PrivateTokenConfiguration()
        {
            IgnoreSslVerification = bool.Parse(privateTokenSection["IgnoreSslVerification"]!),
            AuthenticationUrl = privateTokenSection["AuthenticationUrl"]!,
            ClientSecret = privateTokenSection["ClientSecret"]!,
            ClientId = privateTokenSection["ClientId"]!,
            TokenClaim = privateTokenSection["TokenClaim"]!
        };
        return this;
    }

    public IAuthenticationTokenAccessorBuilder SetAdminTokenInfo(IConfigurationSection adminTokenSection)
    {
        adminToken = new AdminTokenConfiguration()
        {
            IgnoreSslVerification = bool.Parse(adminTokenSection["IgnoreSslVerification"]!),
            AuthenticationUrl = adminTokenSection["AuthenticationUrl"]!,
            Username = adminTokenSection["Username"]!,
            Password = adminTokenSection["Password"]!,
            ClientId = adminTokenSection["ClientId"]!
        };
        return this;
    }

    public AuthenticationTokenAccessor Build()
    {
        return new AuthenticationTokenAccessor(privateToken, adminToken);
    }

}
