using Grpc.Core;

namespace RoomsService.Interfaces
{
    public interface IUserContextService
    {
        string? GetUserId(HttpContext context);
        string? GetUserId(ServerCallContext context);
    }
}