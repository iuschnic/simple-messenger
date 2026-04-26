namespace Auth.BL.Models;
public record RegisterRequest(string UniqueName, string Password, string Email, string DisplayedName);
public record LoginRequest(string UniqueName, string Password);