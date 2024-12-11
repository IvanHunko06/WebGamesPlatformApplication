namespace WebSocketService.Exceptions;

public class InternalServerErrorException : WebSocketServiceException
{
    public InternalServerErrorException() : base()
    {
        
    }
    public InternalServerErrorException(string message) : base(message)
    {
        
    }
}
