namespace ProfileService;

public class UsernameUserContextService
{
    public string? GetUserId(HttpContext context)
    {
        var user = context.User;
        var subjectClaim = user.Claims.FirstOrDefault(c => c.Type == "preferred_username");
        return subjectClaim?.Value;
    }
}
