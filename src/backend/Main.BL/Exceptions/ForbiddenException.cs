namespace Main.BL.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message) : base(message) { }
}
