namespace SharedApiUtils.Abstractons.Authentication;

public class AuthSettings
{
    public IEnumerable<string> ValidIssuers { get; set; }
    public string MetadataAddress { get; set; }
    public string PrivateAudience { get; set; }
    public string PublicAudience { get; set; }
    public bool HttpsMetadata {  get; set; }
    public string AdminRoleClaim {  get; set; }
    public bool IgnoreSslCertificateValidation {  get; set; }
}
