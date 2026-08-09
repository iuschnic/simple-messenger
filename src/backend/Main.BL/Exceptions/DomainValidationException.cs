namespace Main.BL.Exceptions;

public class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message) { }
}
