namespace BL.Exceptions;

public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class BadRequestException : ApiException
{
    public BadRequestException(string message = "Bad request")
        : base(400, message) { }
}

public class UnauthorizedException : ApiException
{
    public UnauthorizedException()
        : base(401, "Unauthorized") { }
}

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message = "Forbidden")
        : base(403, message) { }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message = "Not found")
        : base(404, message) { }
}

public class ConflictException : ApiException
{
    public ConflictException(string message = "Conflict")
        : base(409, message) { }
}

public class ServerException : ApiException
{
    public ServerException(int statusCode, string message = "Server error")
        : base(statusCode, message) { }
}

public class AppException : Exception
{
    public AppException(string message) : base(message) { }
}

public class AuthException : AppException
{
    public AuthException(string message) : base(message) { }
}

public class ValidationException : AppException
{
    public ValidationException(string message) : base(message) { }
}

public class NotFoundAppException : AppException
{
    public NotFoundAppException(string message) : base(message) { }
}