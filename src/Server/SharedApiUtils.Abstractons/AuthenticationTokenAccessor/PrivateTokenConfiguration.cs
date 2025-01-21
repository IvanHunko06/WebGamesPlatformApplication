namespace SharedApiUtils.Abstractons.AuthenticationTokenAccessor;

public class PrivateTokenConfiguration
{
    public bool IgnoreSslVerification { get; set; }
    public string AuthenticationUrl { get; set; }
    public string ClientSecret { get; set; }
    public string ClientId { get; set; }
    public string TokenClaim { get; set; }
}
