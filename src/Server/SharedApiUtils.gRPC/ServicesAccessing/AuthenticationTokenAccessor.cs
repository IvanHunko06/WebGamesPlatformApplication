using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
namespace SharedApiUtils.gRPC.ServicesAccessing;

public class AuthenticationTokenAccessor
{
    private readonly bool ignoreSslVerification;
    private readonly string authenticationUrl;
    private readonly string clientSecret;
    private readonly string clientId;
    private readonly string tokenClaim;
    public AuthenticationTokenAccessor(TokenAccessorConfiguration configuration)
    {
        ignoreSslVerification = configuration.IgnoreSslVerification;
        authenticationUrl = configuration.AuthenticationUrl ?? throw new ArgumentNullException("authentication url is null");
        clientSecret = configuration.ClientSecret ?? throw new ArgumentNullException("client secret is null");
        clientId = configuration.ClientId ?? throw new ArgumentNullException("client id is null");
        tokenClaim = configuration.TokenClaim ?? throw new ArgumentNullException("token claim is null");
    }
    public async Task<string> GetNewToken()
    {
        try
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            if (ignoreSslVerification)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
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
            string? token = responseString;
            if (tokenClaim is not null)
            {
                var jsonObject = JsonSerializer.Deserialize<JsonObject>(responseString);
                if (jsonObject is null) return string.Empty;
                token = jsonObject[tokenClaim]?.GetValue<string>();
            }
            return token ?? string.Empty;
        }
        catch (Exception)
        {
            throw;
        }

    }
}