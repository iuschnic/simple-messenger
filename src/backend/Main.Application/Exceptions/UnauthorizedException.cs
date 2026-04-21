namespace Main.Application.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message) { }
    public UnauthorizedException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not authorized") { }
}
