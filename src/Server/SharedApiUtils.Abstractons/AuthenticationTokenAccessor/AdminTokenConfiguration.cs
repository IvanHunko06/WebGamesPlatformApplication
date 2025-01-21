namespace SharedApiUtils.Abstractons.AuthenticationTokenAccessor;

public class AdminTokenConfiguration
{
    public bool IgnoreSslVerification { get; set; }
    public string AuthenticationUrl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string ClientId { get; set; }
}
