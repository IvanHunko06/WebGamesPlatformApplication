namespace WebSocketService.Exceptions;

public class WebSocketServiceException : Exception
{
    public WebSocketServiceException() : base(){ }
    public WebSocketServiceException(string message) : base(message) { }
}
