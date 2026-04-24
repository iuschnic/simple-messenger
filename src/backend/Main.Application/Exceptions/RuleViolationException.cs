namespace Main.Application.Exceptions;

public class RuleViolationException : AppException
{
    public RuleViolationException(string message) : base(message) { }
}
