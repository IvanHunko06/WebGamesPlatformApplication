using System.Text.Json;
using System.Text.Json.Nodes;
using SharedApiUtils.Abstractons.AuthenticationTokenAccessor;

namespace ProfileService.Services;

public class KeycloakAdmimClient
{
    private readonly Uri eventsUrl;
    private readonly Uri usersUrl;
    private readonly ILogger<KeycloakAdmimClient> logger;
    private readonly AuthenticationTokenAccessor tokenAccessor;

    public KeycloakAdmimClient(
        IConfiguration configuration, 
        ILogger<KeycloakAdmimClient> logger, 
        AuthenticationTokenAccessor tokenAccessor)
    {
        string eventsStringUrl = configuration["KeycloakEventsUrl"]!;
        eventsUrl = new Uri(eventsStringUrl);
        string usersStringUrl = configuration["KeycloakUsersUrl"]!;
        usersUrl = new Uri(usersStringUrl);
        this.logger = logger;
        this.tokenAccessor = tokenAccessor;
    }
    public async Task<string?> GetRegisteredEvents(string eventType)
    {
        try
        {
            string? adminToken = await tokenAccessor.GetAdminToken();
            if (string.IsNullOrWhiteSpace(adminToken))
            {
                logger.LogWarning($"Admin token is null");
                return null;
            }
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, policyErrors) => true;
            HttpClient client = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri($"{eventsUrl.Scheme}://{eventsUrl.Host}:{eventsUrl.Port}"),
            };
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var response = await client.GetAsync($"{eventsUrl.LocalPath}?type={eventType}");
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            }
            logger.LogWarning($"an error occurred while retrieving the list of keycloak events. Http status code: {response.StatusCode}.");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while retrieving the list of keycloak events");
            return null;
        }
    }
    public async Task<JsonObject?> GetUser(string username)
    {
        try
        {
            string? adminToken = await tokenAccessor.GetAdminToken();
            if (string.IsNullOrWhiteSpace(adminToken))
            {
                logger.LogWarning($"Admin token is null");
                return null;
            }
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, policyErrors) => true;
            HttpClient client = new HttpClient(httpClientHandler)
            {
                BaseAddress = new Uri($"{usersUrl.Scheme}://{usersUrl.Host}:{usersUrl.Port}"),
            };
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var response = await client.GetAsync($"{usersUrl.LocalPath}?username={username}");
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                JsonArray? jsonArray = JsonSerializer.Deserialize<JsonArray>(responseBody);
                if(jsonArray is null)
                {
                    logger.LogWarning($"Bad users response body");
                    return null;
                }
                var jsonNode = jsonArray.FirstOrDefault();
                if(jsonNode is null)
                {
                    logger.LogWarning($"Bad users response body");
                    return null;
                }
                return jsonNode.AsObject();
            }
            logger.LogWarning($"an error occurred while retrieving the list of keycloak users. Http status code: {response.StatusCode}.");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while retrieving the list of keycloak users");
            return null;
        }
    }
}
