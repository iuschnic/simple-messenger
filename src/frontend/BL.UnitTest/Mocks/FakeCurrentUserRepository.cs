namespace BL.UnitTest.Mocks;

using BL.Interfaces;
using BL.Models;

public class FakeCurrentUserRepository : ICurrentUserRepository
{
    private CurrentUser _user;

    public Exception? ExceptionToThrow { get; set; }

    private void MaybeThrow()
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;
    }

    public CurrentUser Save(CurrentUser user)
    {
        MaybeThrow();

        _user = user;
        return user;
    }

    public CurrentUser? Get()
    {
        MaybeThrow();

        return _user;
    }

    public void Clear()
    {
        MaybeThrow();

        _user = null;
    }
}