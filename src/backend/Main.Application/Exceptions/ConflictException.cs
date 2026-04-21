namespace Main.Application.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}
