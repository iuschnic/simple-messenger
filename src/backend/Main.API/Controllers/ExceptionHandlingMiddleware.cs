using Main.BL.Exceptions;
using Main.Application.Exceptions;
using System.Security.Claims;
using System.Text.Json;

namespace Main.API.Controllers;

using ILogger = Serilog.ILogger;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly IWebHostEnvironment _env;
    private readonly JsonSerializerOptions _jsonOptions;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        };
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

        var traceId = context.TraceIdentifier;
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var method = context.Request.Method;
        var path = context.Request.Path;

        var errorResponse = new ErrorResponse
        {
            TraceId = traceId
        };

        switch (exception)
        {
            case DomainValidationException ex:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "INVALID_ARGUMENT";
                _logger.Information("Invalid argument: {ArgumentError}", ex.Message);
                break;

            case UnauthorizedException ex:
                response.StatusCode = StatusCodes.Status401Unauthorized;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "UNAUTHORIZED";
                _logger.Warning("Unauthorized access: {Message}", ex.Message);
                break;

            case ForbiddenException ex:
                response.StatusCode = StatusCodes.Status403Forbidden;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "FORBIDDEN";
                _logger.Warning("Forbidden access for user {UserId} to {Path}", userId, path);
                break;

            case NotFoundException ex:
                response.StatusCode = StatusCodes.Status404NotFound;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "NOT_FOUND";
                _logger.Information("Resource not found: {Resource}", ex.Message);
                break;

            case ConflictException ex:
                response.StatusCode = StatusCodes.Status409Conflict;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "CONFLICT";
                _logger.Information("Conflict detected: {ConflictDetails}", ex.Message);
                break;

            case RuleViolationException ex:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "RULE_VIOLATION";
                _logger.Information("Business rule violation: {RuleName}", ex.Message);
                break;

            case TechnicalException ex:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                errorResponse.Error = _env.IsDevelopment()
                    ? ex.Message
                    : "A technical error occurred";
                errorResponse.Code = "TECHNICAL_ERROR";
                _logger.Error(ex, "Technical error at {Method} {Path}: {ErrorMessage}",
                    method, path, ex.Message);

                if (_env.IsDevelopment() && ex.InnerException != null)
                {
                    _logger.Debug("Inner exception: {InnerMessage}", ex.InnerException.Message);
                    errorResponse.InnerError = ex.InnerException.Message;
                }
                break;

            case DomainException ex:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Code = "DOMAIN_ERROR";
                _logger.Warning("Unmapped DomainException subtype {ExceptionType}, {Message}", 
                    ex.GetType().Name, ex.Message);
                break;

            case AppException ex:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Error = ex.Message;
                errorResponse.Code = "APPLICATION_ERROR";
                _logger.Warning("Unmapped AppException subtype {ExceptionType}, {Message}",
                    ex.GetType().Name, ex.Message);
                break;

            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                errorResponse.Error = _env.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred";
                errorResponse.Code = "INTERNAL_ERROR";
                _logger.Error(exception,
                    "Unhandled exception at {Method} {Path}: {ExceptionType} - {Message}",
                    method, path, exception.GetType().Name, exception.Message);
                break;
        }
        if (_env.IsDevelopment())
        {
            errorResponse.StackTrace = exception.StackTrace;
            if (exception.InnerException != null)
            {
                errorResponse.InnerError = exception.InnerException.Message;
            }
        }
        else
        {
            if (exception is not AppException)
                _logger.Debug("Stack trace: {StackTrace}", exception.StackTrace);
        }
        await response.WriteAsJsonAsync(errorResponse, _jsonOptions);
    }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerError { get; set; }
}