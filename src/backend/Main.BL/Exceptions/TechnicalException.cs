namespace Main.BL.Exceptions;

public class TechnicalException : AppException
{
    public TechnicalException(string message) : base(message) { }
    public TechnicalException(string message, Exception inner) : base(message, inner) { }
}
