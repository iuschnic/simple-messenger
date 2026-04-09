namespace Main.BL.Exceptions;

public class RuleViolationException : AppException
{
    public RuleViolationException(string message) : base(message) { }
}
