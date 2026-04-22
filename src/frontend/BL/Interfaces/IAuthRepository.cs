using BL.Models;

namespace BL.Interfaces;

public interface IAuthRepository
{
    Task<User> Register(string uniqueName, string passwordHash, string email);
    Task<User?> Authenticate(string uniqueName, string passwordHash);
    Task<User?> Get(Guid id);
}