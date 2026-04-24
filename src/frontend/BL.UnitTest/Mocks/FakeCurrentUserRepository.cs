namespace BL.UnitTest.Mocks;

using BL.Interfaces;
using BL.Models;

public class FakeCurrentUserRepository : ICurrentUserRepository
{
    private CurrentUser? _user;

    public Exception? ExceptionToThrow { get; set; }

    private void MaybeThrow()
    {
        if (ExceptionToThrow != null)
            throw ExceptionToThrow;
    }

    public Task<CurrentUser> Save(CurrentUser user)
    {
        MaybeThrow();

        _user = user;
        return Task.FromResult(user);
    }

    public Task<CurrentUser?> Get()
    {
        MaybeThrow();

        return Task.FromResult(_user);
    }

    public Task Clear()
    {
        MaybeThrow();

        _user = null;
        return Task.CompletedTask;
    }
}