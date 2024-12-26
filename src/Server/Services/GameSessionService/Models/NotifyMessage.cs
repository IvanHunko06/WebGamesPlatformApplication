namespace GameSessionService.Models;

public class NotifyMessage
{
    public bool IsSuccess {  get; set; }
    public string? GameErrorMessage {  get; set; }
    public string? Payload {  get; set; }
}
