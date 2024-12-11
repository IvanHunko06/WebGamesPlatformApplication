namespace WebSocketService.Hubs;

public record HubActionResult(bool isSuccess, string? errorMessage);