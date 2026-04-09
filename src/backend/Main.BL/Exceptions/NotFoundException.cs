namespace Main.BL.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id '{id}' was not found") { }

    public NotFoundException(string message) : base(message) { }
}