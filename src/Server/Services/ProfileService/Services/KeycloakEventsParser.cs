using System.Text.Json.Nodes;

namespace ProfileService.Services;

public class KeycloakEventsParser
{
    private readonly ILogger<KeycloakEventsParser> logger;

    public KeycloakEventsParser(ILogger<KeycloakEventsParser> logger)
    {
        this.logger = logger;
    }
    public List<string> ParseLoginEvents(JsonArray loginEventsArray)
    {
        try
        {
            List<string> usernames = new List<string>();
            foreach (var loginEvent in loginEventsArray)
            {
                if (loginEvent is null) continue;
                string clientId = loginEvent["clientId"]!.GetValue<string>();
                if (clientId != "public-client") continue;
                JsonObject details = loginEvent["details"]!.AsObject();
                string username = details["username"]!.GetValue<string>();
                usernames.Add(username);
            }
            return usernames;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while parsing the LOGIN events");
            return new List<string>();
        }
    }
    public List<string> ParseRegisterEvents(JsonArray registerEventsArray)
    {
        try
        {
            List<string> usernames = new List<string>();
            foreach (var loginEvent in registerEventsArray)
            {
                if (loginEvent is null) continue;
                string clientId = loginEvent["clientId"]!.GetValue<string>();
                if (clientId != "public-client") continue;
                JsonObject details = loginEvent["details"]!.AsObject();
                string username = details["username"]!.GetValue<string>();
                usernames.Add(username);
            }
            return usernames;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "an error occurred while parsing the LOGIN events");
            return new List<string>();
        }
    }
}
