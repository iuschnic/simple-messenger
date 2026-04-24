namespace BL.Interfaces;

public class RepositoryHub
{
    public IAuthRepository Auth { get; }
    public IUserRepository Users { get; }
    public IChatRepository Chats { get; }
    public IMessageRepository Messages { get; }
    public ICurrentUserRepository CurrentUser { get; }

    public RepositoryHub(
        IAuthRepository auth,
        IUserRepository users,
        IChatRepository chats,
        IMessageRepository messages,
        ICurrentUserRepository currentUser)
    {
        Auth = auth;
        Users = users;
        Chats = chats;
        Messages = messages;
        CurrentUser = currentUser;
    }
}