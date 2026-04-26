using Microsoft.AspNetCore.Mvc;
using Auth.BL.Models;

namespace Auth.BL.InputPorts
{
    public interface IAuthService
    {
        Task<ActionResult> RegisterAsync(RegisterRequest registerRequest);
        Task<ActionResult> LoginAsync(LoginRequest loginRequest);
    }
}