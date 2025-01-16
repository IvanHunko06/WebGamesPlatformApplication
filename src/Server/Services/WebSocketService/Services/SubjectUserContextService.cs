using Microsoft.AspNetCore.SignalR;
using SharedApiUtils;
using System.Security.Claims;
using WebSocketService.Hubs;
using WebSocketService.Interfaces;

namespace WebSocketService.Services;

public class SubjectUserContextService : IUserContextService
{
    public string? GetUserId(HubCallerContext context)
    {
        if (context.User is null)
            return null;

        var subjectClaim = context.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
        if (subjectClaim is null)
            return null;

        return subjectClaim.Value;
    }
}
