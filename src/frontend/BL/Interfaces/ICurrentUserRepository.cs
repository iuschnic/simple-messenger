using BL.Models;

namespace BL.Interfaces;

public interface ICurrentUserRepository
{
    CurrentUser Save(CurrentUser user);
    CurrentUser? Get();
    void Clear();
}