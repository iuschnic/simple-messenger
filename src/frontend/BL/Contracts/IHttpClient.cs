using BL.Models;

namespace BL.Contracts;

public interface IHttpClient
{
    // AUTH
    HttpResponseMessage Register(string uniqueName, string password, string email, string displayedName);
    string Login(string uniqueName, string password);
    public User GetMe();
    User GetUserByName(string uniqueName);
    public CurrentUser UpdateMeDisplayName(string displayName);
    public User UpdateContactName(Guid id, string contactName);

    // ================= USERS =================
    User GetUser(Guid id);
    List<User> SearchUsers(string substr, int maxUsers);

    // ================= CHATS =================
    List<Chat> GetChats();
    public Chat CreateGroupChat(string name, List<Guid> memberIds);
    Chat CreatePrivateChat(Guid withUserId);
    public Chat GetChat(Guid chatId);
    public List<SyncChatResult> SyncChats(List<(Guid chatId, long version)> chats);

    public SyncChatResult RemoveUserFromChat(Guid chatId, Guid userId);

    // ================= MESSAGES =================
    SyncChatResult SendMessage(Guid chatId, string text, long clientVersion);

    SyncChatResult EditMessage(Guid chatId, long messageNum, string newText, long clientVersion);

    SyncChatResult DeleteMessage(Guid chatId, long messageNum, long clientVersion);

    List<Message> GetMessages(Guid chatId, long? fromMessageNumber = null, int? limit = null);
}