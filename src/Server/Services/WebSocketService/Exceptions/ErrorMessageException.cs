namespace WebSocketService.Exceptions;

public class ErrorMessageException : WebSocketServiceException
{
    public ErrorMessageException(string errorMessage) : base(errorMessage)
    {
        
    }
}
