namespace WebSocketService.Hubs;

public class HubActionResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public object? Payload { get; set; }
    public HubActionResult(bool isSuccess, string? errorMessage, object? payload)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Payload = payload;
    }
}