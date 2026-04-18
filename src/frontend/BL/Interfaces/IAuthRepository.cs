using BL.Models;

namespace BL.Interfaces;

public interface IAuthRepository
{
    User Register(string uniqueName, string passwordHash, string email);
    User Authenticate(string uniqueName, string passwordHash);
    User Get(Guid id);
}