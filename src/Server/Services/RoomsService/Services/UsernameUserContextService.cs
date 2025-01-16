using System.Security.Claims;
using Grpc.Core;
using RoomsService.Interfaces;

namespace RoomsService.Services;

public class UsernameUserContextService : IUserContextService
{
    public string? GetUserId(ServerCallContext context)
    {
        var user = context.GetHttpContext().User;
        var subjectClaim = user.Claims.FirstOrDefault(c => c.Type == "preferred_username");
        return subjectClaim?.Value;
    }
    public string? GetUserId(HttpContext context)
    {
        var user = context.User;
        var subjectClaim = user.Claims.FirstOrDefault(c => c.Type == "preferred_username");
        return subjectClaim?.Value;
    }
}
