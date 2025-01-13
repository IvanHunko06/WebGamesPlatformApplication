namespace SharedApiUtils.gRPC.ServicesAccessing;

public class TokenAccessorConfiguration
{
    public bool IgnoreSslVerification { get; set; }
    public string? AuthenticationUrl { get; set; } = null;
    public string? ClientSecret { get; set; } = null;
    public string? ClientId { get; set; } = null;
    public string? TokenClaim { get; set; } = null;
}
