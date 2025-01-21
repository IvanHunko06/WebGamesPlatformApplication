using System.Text.Json;
using System.Text.Json.Nodes;
using ProfileService.Interfaces;
using SharedApiUtils.Abstractons.AuthenticationTokenAccessor;

namespace ProfileService.Services;

public class KeycloakEventsBackgroundListener : BackgroundService
{
    private KeycloakAdmimClient keycloakAdminClient;
    private KeycloakEventsParser keycloakEventsParser;
    private IProfileService profileService;
    private readonly ILogger<KeycloakEventsBackgroundListener> logger;
    private readonly IServiceProvider serviceProvider;

    public KeycloakEventsBackgroundListener(
        ILogger<KeycloakEventsBackgroundListener> logger,
        IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = serviceProvider.CreateScope())
        {
            keycloakAdminClient = scope.ServiceProvider.GetRequiredService<KeycloakAdmimClient>();
            keycloakEventsParser = scope.ServiceProvider.GetRequiredService<KeycloakEventsParser>();
            profileService = scope.ServiceProvider.GetRequiredService<IProfileService>();
            while (!stoppingToken.IsCancellationRequested)
            {
                logger.LogInformation($"Trying to get new keycloak events...");
                await GetAndHandleEvents();
                logger.LogInformation($"Delay 2 minutes task");
                await Task.Delay(TimeSpan.FromMinutes(2));
            }
        }

    }
    private async Task GetAndHandleEvents()
    {
        try
        {
            string? loginEvents = await keycloakAdminClient.GetRegisteredEvents("LOGIN");
            if (!string.IsNullOrEmpty(loginEvents))
            {
                var loginEventsArray = JsonSerializer.Deserialize<JsonArray>(loginEvents);
                if (loginEventsArray is not null)
                {
                    var loginedUsers = keycloakEventsParser.ParseLoginEvents(loginEventsArray);
                    if (loginedUsers is not null && loginedUsers.Count > 0)
                        await profileService.CreateDefaultProfiles(loginedUsers);
                    else
                        logger.LogWarning("Logined users list is null or empty");
                }
                else
                {
                    logger.LogWarning($"Login events json array is null");
                }
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing LOGIN events.");
        }

        try
        {
            string? registerEvents = await keycloakAdminClient.GetRegisteredEvents("REGISTER");
            if (!string.IsNullOrEmpty(registerEvents))
            {
                var registerEventsArray = JsonSerializer.Deserialize<JsonArray>(registerEvents);
                if (registerEventsArray is not null)
                {
                    var registeredUsers = keycloakEventsParser.ParseRegisterEvents(registerEventsArray);
                    if (registeredUsers is not null && registeredUsers.Count > 0)
                        await profileService.CreateDefaultProfiles(registeredUsers);
                    else
                        logger.LogWarning($"Registered users list is null or empty");
                }
                else
                {
                    logger.LogWarning($"Register events json array is null");
                }
            }
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing REGISTER events.");
        }
    }
}
