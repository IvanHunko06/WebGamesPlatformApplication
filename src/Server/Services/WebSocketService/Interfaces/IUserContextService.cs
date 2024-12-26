using Microsoft.AspNetCore.SignalR;

namespace WebSocketService.Interfaces;

public interface IUserContextService
{
    string? GetUserId(HubCallerContext context);
}
