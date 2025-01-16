using System.Collections.Concurrent;
using WebSocketService.Services;

namespace WebSocketService.HubStates;

public class SessionManagmentHubState
{
    public UserConnectionStateService UserConnections { get; private set; }
    public ConcurrentDictionary<string, CancellationTokenSource> UserDisconnectionTokens { get; private set; }
    public List<string> BlockedClients { get; private set; }
    public SessionManagmentHubState(UserConnectionStateService userConnectionStateService)
    {
        UserConnections = userConnectionStateService;
        UserDisconnectionTokens = new ConcurrentDictionary<string, CancellationTokenSource>();
        BlockedClients = new List<string>();
    }

}
