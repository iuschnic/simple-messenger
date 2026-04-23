using Main.Application.Exceptions;
using System.Security.Claims;

namespace Main.API.Controllers;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst("userId")?.Value
                       ?? principal.FindFirst("id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedException("User ID not found in token");
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException($"Invalid user ID format: {userIdClaim}");
        return userId;
    }
}
