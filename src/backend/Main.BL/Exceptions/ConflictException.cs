namespace Main.BL.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}
