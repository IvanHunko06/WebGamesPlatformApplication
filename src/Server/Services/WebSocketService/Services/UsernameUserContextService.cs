using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using WebSocketService.Interfaces;

namespace WebSocketService.Services;

public class UsernameUserContextService : IUserContextService
{
    public string? GetUserId(HubCallerContext context)
    {
        if (context.User is null)
            return null;

        var subjectClaim = context.User.Claims.Where(c => c.Type == "preferred_username").FirstOrDefault();
        if (subjectClaim is null)
            return null;

        return subjectClaim.Value;
    }
}
