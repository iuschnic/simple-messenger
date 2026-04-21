using BL.Interfaces;

namespace BL.UnitTest.Mocks;

public class FakeRepositoryHub : RepositoryHub
{
    public FakeRepositoryHub()
        : base(
            new FakeAuthRepository(),
            new FakeUserRepository(),
            new FakeChatRepository(),
            new FakeMessageRepository(),
            new FakeCurrentUserRepository())
    {
    }
}