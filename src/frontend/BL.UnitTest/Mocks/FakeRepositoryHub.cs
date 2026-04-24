using BL.Interfaces;

namespace BL.UnitTest.Mocks;

public class FakeRepositoryHub : RepositoryHub
{
    public FakeAuthRepository AuthRepo { get; }
    public FakeUserRepository UsersRepo { get; }
    public FakeChatRepository ChatsRepo { get; }
    public FakeMessageRepository MessagesRepo { get; }
    public FakeCurrentUserRepository CurrentUserRepo { get; }

    public FakeRepositoryHub()
        : this(
            new FakeAuthRepository(),
            new FakeUserRepository(),
            new FakeChatRepository(),
            new FakeMessageRepository(),
            new FakeCurrentUserRepository())
    {
    }

    private FakeRepositoryHub(
        FakeAuthRepository auth,
        FakeUserRepository users,
        FakeChatRepository chats,
        FakeMessageRepository messages,
        FakeCurrentUserRepository currentUser)
        : base(auth, users, chats, messages, currentUser)
    {
        AuthRepo = auth;
        UsersRepo = users;
        ChatsRepo = chats;
        MessagesRepo = messages;
        CurrentUserRepo = currentUser;
    }

    public Exception? ExceptionToThrow
    {
        set
        {
            AuthRepo.ExceptionToThrow = value;
            UsersRepo.ExceptionToThrow = value;
            ChatsRepo.ExceptionToThrow = value;
            MessagesRepo.ExceptionToThrow = value;
            CurrentUserRepo.ExceptionToThrow = value;
        }
    }
}