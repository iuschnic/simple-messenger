namespace Main.Application.Exceptions;

public class DeserializeException : AppException
{
    public DeserializeException() : base("Failed to deserialize message into DTO") { }
}
