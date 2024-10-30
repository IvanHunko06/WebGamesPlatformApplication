using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace RoomsService;

public class AuthenticationTokenAccessor
{
    private readonly IConfiguration configuration;
    private readonly ILogger<AuthenticationTokenAccessor> logger;
    private readonly IConfigurationSection accessTokenSection;
    private readonly bool ignoreSslVerification;
    private readonly string authenticationUrl;
    private readonly string clientSecret;
    private readonly string clientId;
    private readonly string tokenClaim;
    public AuthenticationTokenAccessor(IConfiguration configuration, ILogger<AuthenticationTokenAccessor> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
        this.accessTokenSection = configuration.GetRequiredSection("PrivateAccessToken");
        this.ignoreSslVerification = accessTokenSection.GetValue<bool>("IgnoreSslVerification");
        this.authenticationUrl = accessTokenSection.GetValue<string>("AuthenticationUrl") ?? throw new ArgumentNullException("authentication url is null");
        this.clientSecret = accessTokenSection.GetValue<string>("ClientSecret") ?? throw new ArgumentNullException("client secret is null");
        this.clientId = accessTokenSection.GetValue<string>("ClientId") ?? throw new ArgumentNullException("client id is null");
        this.tokenClaim = accessTokenSection.GetValue<string>("TokenClaim") ?? throw new ArgumentNullException("token claim is null");
    }
    public async Task<string> GetNewToken()
    {
        try
        {
            HttpClientHandler httpClientHandler;
            if (ignoreSslVerification)
            {
                httpClientHandler = new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
            }
            else
            {
                httpClientHandler = new HttpClientHandler();
            }
            HttpClient httpClient = new HttpClient(httpClientHandler);
            var parameters = new Dictionary<string, string>
            {
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"grant_type", "client_credentials"},
                {"scope","openid" }
            };
            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync(authenticationUrl, content);
            if (!response.IsSuccessStatusCode) return string.Empty;

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(responseString);
            if (jsonObject is null) return string.Empty;
            string? token = jsonObject[tokenClaim]?.GetValue<string>();

            return token ?? string.Empty;
        }catch(Exception ex)
        {
            logger.LogError(ex.ToString());
            return string.Empty;
        }
        
    }
}
