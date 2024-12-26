using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SharedApiUtils;
using WebSocketService.Clients;
namespace WebSocketService.Hubs;

[Authorize(Policy = "OnlyPublicClient")]
public class RoomsHub : Hub<IRoomsClient>
{
    private readonly GameIdsList gameIds;

    public RoomsHub(GameIdsList gameIds)
    {
        this.gameIds = gameIds;
    }
    public async Task<HubActionResult> ListenRooms(string gameId)
    {
        List<string> gameIdsList = await gameIds.Get();
        if (gameIdsList.Contains(gameId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            return new HubActionResult(true, null, null);
        }
        else
        {
            return new HubActionResult(false, ErrorMessages.GameIdNotValid, null);
        }

    }
}
