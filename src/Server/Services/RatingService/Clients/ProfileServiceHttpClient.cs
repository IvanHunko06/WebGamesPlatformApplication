using System.Text.Json;
using System.Text.Json.Nodes;
using SharedApiUtils.Abstractons.AuthenticationTokenAccessor;
using SharedApiUtils.Abstractons.ExternalServices;

namespace RatingService.Clients;

public class ProfileServiceHttpClient
{
    private readonly AccessingConfiguration accessingConfiguration;
    private readonly ILogger<ProfileServiceHttpClient> logger;
    private readonly AuthenticationTokenAccessor authenticationTokenAccessor;
    private readonly Uri profileServiceUri;

    public ProfileServiceHttpClient(
        AccessingConfiguration accessingConfiguration, 
        ILogger<ProfileServiceHttpClient> logger,
        AuthenticationTokenAccessor authenticationTokenAccessor)
    {
        this.accessingConfiguration = accessingConfiguration;
        this.logger = logger;
        this.authenticationTokenAccessor = authenticationTokenAccessor;
        this.profileServiceUri = new Uri(accessingConfiguration.ProfileServiceUrl ?? throw new ArgumentNullException("Profile service url is null"));
    }
    public async Task<bool?> UserProfileIsPrivate(string userId)
    {
        try
        {
            string authToken = await authenticationTokenAccessor.GetPrivateClientToken();
            HttpClientHandler handler = new HttpClientHandler();
            if (accessingConfiguration.IgnoreSslVerification)
                handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, policyErrors) => true;

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = profileServiceUri;
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await client.GetAsync($"profile/{userId}/privacy");
            if (!response.IsSuccessStatusCode)
                return null;

            string responseBody = await response.Content.ReadAsStringAsync();
            var responseJsonObject = JsonSerializer.Deserialize<JsonObject>(responseBody);
            if (responseJsonObject is null)
            {
                logger.LogWarning($"Response json object is null");
                return null;
            }
            bool isPrivateProfile = responseJsonObject["isPrivate"]!.GetValue<bool>();
            return isPrivateProfile;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "an error occurred while retrieving user privacy");
            return null;
        }
    }
}
