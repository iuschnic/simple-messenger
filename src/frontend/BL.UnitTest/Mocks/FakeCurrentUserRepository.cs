namespace BL.UnitTest.Mocks;

using BL.Interfaces;
using BL.Models;

public class FakeCurrentUserRepository : ICurrentUserRepository
{
    private CurrentUser _user;

    public CurrentUser Save(CurrentUser user)
    {
        _user = user;
        return user;
    }

    public CurrentUser? Get()
        => _user;

    public void Clear()
        => _user = null;
}