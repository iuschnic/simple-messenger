using BL.Models;

namespace BL.Contracts;

public interface IHttpClient
{
    // ================= AUTH =================

    Task Register(string uniqueName, string password, string email, string displayedName);
    Task<string> Login(string uniqueName, string password);
    Task<User> GetMe();
    Task<User> GetUserByName(string uniqueName);
    Task<CurrentUser> UpdateMeDisplayName(string displayName);
    Task<User> UpdateContactName(Guid id, string contactName);
    Task<List<User>> GetContacts();
    
    // ================= USERS =================

    Task<User> GetUser(Guid id);
    Task<List<User>> SearchUsers(string substr, int maxUsers);

    // ================= CHATS =================

    Task<List<Chat>> GetChats();
    Task<Chat> CreateGroupChat(string name, List<Guid> memberIds);
    Task<Chat> CreatePrivateChat(Guid withUserId);
    Task<Chat> GetChat(Guid chatId);
    Task<SyncChatResult> SyncChat(Guid chatId, ulong clientVersion);
    Task<List<SyncChatResult>> SyncChats(List<(Guid chatId, ulong version)> chats);
    Task<SyncChatResult> RemoveUserFromChat(Guid chatId, Guid userId, ulong clientVersion);

    // ================= MESSAGES =================

    Task<SyncChatResult> SendMessage(Guid chatId, string text, ulong clientVersion);
    Task<SyncChatResult> EditMessage(Guid chatId, ulong messageNum, string newText, ulong clientVersion);
    Task<SyncChatResult> DeleteMessage(Guid chatId, ulong messageNum, ulong clientVersion);
    Task<List<Message>> GetMessages(Guid chatId, ulong fromMessageNumber, int limit);

    Task<User> AddContact(Guid userContactId, string contactName);
}