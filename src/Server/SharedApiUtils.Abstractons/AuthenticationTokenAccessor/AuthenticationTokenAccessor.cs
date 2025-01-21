using System.Text.Json;
using System.Text.Json.Nodes;
namespace SharedApiUtils.Abstractons.AuthenticationTokenAccessor;

public class AuthenticationTokenAccessor
{
    private readonly PrivateTokenConfiguration? privateToken;
    private readonly AdminTokenConfiguration? adminToken;

    public AuthenticationTokenAccessor(PrivateTokenConfiguration? privateToken, AdminTokenConfiguration? adminToken)
    {
        this.privateToken = privateToken;
        this.adminToken = adminToken;
    }
    public async Task<string> GetPrivateClientToken()
    {
        if (privateToken is null)
            throw new ArgumentNullException("Private token configuration not found");
        try
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            if (privateToken.IgnoreSslVerification)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
            HttpClient httpClient = new HttpClient(httpClientHandler);
            var parameters = new Dictionary<string, string>
            {
                {"client_id", privateToken.ClientId},
                {"client_secret", privateToken.ClientSecret},
                {"grant_type", "client_credentials"},
                {"scope","openid" }
            };
            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync(privateToken.AuthenticationUrl, content);
            if (!response.IsSuccessStatusCode) return string.Empty;

            var responseString = await response.Content.ReadAsStringAsync();
            string? token = responseString;
            if (privateToken.TokenClaim is not null)
            {
                var jsonObject = JsonSerializer.Deserialize<JsonObject>(responseString);
                if (jsonObject is null) return string.Empty;
                token = jsonObject[privateToken.TokenClaim]?.GetValue<string>();
            }
            return token ?? string.Empty;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<string> GetAdminToken()
    {
        if (adminToken is null)
            throw new ArgumentNullException("Admin token configuration not found");
        try
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            if (adminToken.IgnoreSslVerification)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }
            HttpClient httpClient = new HttpClient(httpClientHandler);
            var parameters = new Dictionary<string, string>
            {
                {"client_id", adminToken.ClientId},
                {"username", adminToken.Username},
                {"grant_type", "password"},
                {"password",adminToken.Password }
            };
            var content = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync(adminToken.AuthenticationUrl, content);
            if (!response.IsSuccessStatusCode) return string.Empty;

            var responseString = await response.Content.ReadAsStringAsync();
            string? token = responseString;
            var jsonObject = JsonSerializer.Deserialize<JsonObject>(responseString);
            if (jsonObject is null) return string.Empty;
            token = jsonObject["access_token"]?.GetValue<string>();
            return token ?? string.Empty;
        }
        catch (Exception)
        {
            throw;
        }

    }
}