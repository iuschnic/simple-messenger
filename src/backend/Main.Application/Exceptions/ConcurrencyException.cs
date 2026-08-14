namespace Main.Application.Exceptions;

public class ConcurrencyException : AppException
{
    public ConcurrencyException(string message) : base(message) { }
}
