namespace WebSocketService;

public class JwtFromUriMiddleware
{
    private readonly RequestDelegate _next;

    public JwtFromUriMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Query.ContainsKey("access_token"))
        {
            var token = context.Request.Query["access_token"].ToString();

            if (!string.IsNullOrEmpty(token))
            {
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }
        }

        await _next(context);
    }
}
