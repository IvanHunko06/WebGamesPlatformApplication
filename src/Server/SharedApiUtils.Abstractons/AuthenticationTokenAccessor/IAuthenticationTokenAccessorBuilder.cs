using Microsoft.Extensions.Configuration;

namespace SharedApiUtils.Abstractons.AuthenticationTokenAccessor;

public interface IAuthenticationTokenAccessorBuilder
{
    IAuthenticationTokenAccessorBuilder SetPrivateTokenInfo(IConfigurationSection privateTokenSection);
    IAuthenticationTokenAccessorBuilder SetAdminTokenInfo(IConfigurationSection adminTokenSection);
    AuthenticationTokenAccessor Build();
}
