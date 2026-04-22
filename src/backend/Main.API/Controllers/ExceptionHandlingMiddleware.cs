using System.Text.Json;
using Main.Application.Exceptions;

namespace Main.Presentation.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";
        var errorResponse = new ErrorResponse
        {
            TraceId = context.TraceIdentifier
        };
        switch (exception)
        {
            case UnauthorizedException ex:
                response.StatusCode = StatusCodes.Status401Unauthorized;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "UNAUTHORIZED";
                _logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
                break;

            case ForbiddenException ex:
                response.StatusCode = StatusCodes.Status403Forbidden;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "FORBIDDEN";
                _logger.LogWarning(ex, "Forbidden access: {Message}", ex.Message);
                break;

            case NotFoundException ex:
                response.StatusCode = StatusCodes.Status404NotFound;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "NOT_FOUND";
                _logger.LogInformation(ex, "Resource not found: {Message}", ex.Message);
                break;

            case ConflictException ex:
                response.StatusCode = StatusCodes.Status409Conflict;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "CONFLICT";
                _logger.LogInformation(ex, "Conflict: {Message}", ex.Message);
                break;

            case RuleViolationException ex:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "RULE_VIOLATION";
                _logger.LogInformation(ex, "Business rule violation: {Message}", ex.Message);
                break;

            case ArgumentException ex:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "INVALID_ARGUMENT";
                _logger.LogInformation(ex, "Invalid argument: {Message}", ex.Message);
                break;

            case TechnicalException ex:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                errorResponse.Error = _env.IsDevelopment()
                    ? ex.Message
                    : "A technical error occurred";
                errorResponse.Code = "TECHNICAL_ERROR";
                _logger.LogError(ex, "Technical error: {Message}", ex.Message);
                if (_env.IsDevelopment() && ex.InnerException != null)
                    errorResponse.InnerError = ex.InnerException.Message;
                break;

            case AppException ex:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "APPLICATION_ERROR";
                _logger.LogWarning(ex, "Application error: {Message}", ex.Message);
                break;

            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                errorResponse.Error = _env.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred";
                errorResponse.Code = "INTERNAL_ERROR";
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }
        if (_env.IsDevelopment())
        {
            errorResponse.StackTrace = exception.StackTrace;
            if (exception.InnerException != null)
                errorResponse.InnerError = exception.InnerException.Message;
        }
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        };
        await response.WriteAsJsonAsync(errorResponse, jsonOptions);
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public object? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public string? StackTrace { get; set; }
    public string? InnerError { get; set; }
}