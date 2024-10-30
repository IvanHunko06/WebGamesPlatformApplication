using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebSocketService.Hubs;

[Authorize(Policy = "AllAuthenticatedUsers")]
public class MainHub :Hub
{
    private readonly ILogger<MainHub> logger;

    public MainHub(ILogger<MainHub> logger)
    {
        this.logger = logger;
    }
    public override async Task OnConnectedAsync()
    {
        logger.LogInformation($"{Context.ConnectionId} has connected to Main Hub");
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        logger.LogInformation($"{Context.ConnectionId} has disconnected from Main Hub");
    }
}
