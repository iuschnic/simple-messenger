namespace Main.BL.Exceptions;

public class DomainRuleViolationException : DomainException
{
    public DomainRuleViolationException(string message) : base(message) { }
}
