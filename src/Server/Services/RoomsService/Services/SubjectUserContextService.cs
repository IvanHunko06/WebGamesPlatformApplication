using Grpc.Core;
using RoomsService.Interfaces;
using System.Security.Claims;

namespace RoomsService.Services;

public class SubjectUserContextService : IUserContextService
{
    public string? GetUserId(ServerCallContext context)
    {
        var user = context.GetHttpContext().User;
        var subjectClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        return subjectClaim?.Value;
    }
    public string? GetUserId(HttpContext context)
    {
        var user = context.User;
        var subjectClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        return subjectClaim?.Value;
    }
}
