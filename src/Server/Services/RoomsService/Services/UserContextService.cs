using Grpc.Core;
using System.Security.Claims;

namespace RoomsService.Services;

public class UserContextService
{
    public string? GetUserId(ServerCallContext context)
    {
        var user = context.GetHttpContext().User;
        var subjectClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        return subjectClaim?.Value;
    }
}
