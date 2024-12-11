using WebSocketService.Services;

namespace WebSocketService.HubStates;

public class SessionManagmentHubState
{
    public UserConnectionStateService UserConnections { get; private set; }
    public List<string> ConnectedClients { get; private set; }
    public SessionManagmentHubState(UserConnectionStateService userConnectionStateService)
    {
        UserConnections = userConnectionStateService;
        ConnectedClients = new List<string>();
    }

}
