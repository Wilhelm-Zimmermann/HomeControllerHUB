namespace HomeControllerHUB.Domain.Models;

public class AppError : Exception
{
    public int StatusCode { get; private set; }
    public string Message { get; private set; }
    public string Description { get; private set; }

    public AppError(int statusCode, string message) : this(statusCode, message, "")
    {
        
    }
    
    public AppError(int statusCode, string message, string description)
    {
        StatusCode = statusCode;
        Message = message;
        Description = description;
    }
}