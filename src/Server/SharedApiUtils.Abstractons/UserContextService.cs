using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SharedApiUtils.Abstractons;

public class UserContextService
{
    public string? GetUserId(HttpContext context)
    {
        var user = context.User;
        var subjectClaim = user.Claims.FirstOrDefault(c => c.Type == "preferred_username");
        return subjectClaim?.Value;
    }
    public string? GetUserId(ClaimsPrincipal? user)
    {
        if(user is null)return null;
        var subjectClaim = user.Claims.FirstOrDefault(c => c.Type == "preferred_username");
        return subjectClaim?.Value;
    }
}
