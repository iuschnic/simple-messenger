using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Main.API.Controllers;

public class ApiKeyAuthFilter : IAsyncAuthorizationFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyAuthFilter> _logger;
    private const string ApiKeyHeader = "X-Api-Key";

    public ApiKeyAuthFilter(IConfiguration configuration, ILogger<ApiKeyAuthFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var extractedApiKey))
        {
            _logger.LogError("Missing API key for internal endpoint {Path}",
                context.HttpContext.Request.Path);
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "API key is missing",
                code = "MISSING_API_KEY"
            });
            return;
        }
        var apiKey = _configuration.GetValue<string>("InternalApiKey");
        if (string.IsNullOrEmpty(apiKey) || !apiKey.Equals(extractedApiKey))
        {
            _logger.LogError("Invalid API key for internal endpoint {Path}",
                context.HttpContext.Request.Path);

            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Invalid API key",
                code = "INVALID_API_KEY"
            });
            return;
        }
        var claims = new[] { new Claim("IsInternalCall", "true") };
        var identity = new ClaimsIdentity(claims, "ApiKey");
        context.HttpContext.User = new ClaimsPrincipal(identity);
    }
}
