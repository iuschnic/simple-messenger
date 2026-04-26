using BL.Models;

namespace BL.Interfaces;

public interface ICurrentUserRepository
{
    Task<CurrentUser?> Save(CurrentUser user);
    Task<CurrentUser?> Get();
    Task Clear();
}