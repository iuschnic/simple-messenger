using BL.Models;

namespace BL.Interfaces;

public interface IMessengerUI
{
    // ================= AUTH =================
    User RegisterUser(string uniqueName, string password, string email, string displayName);
    User Login(string uniqueName, string password);
    User GetCurrentUser();
    User UpdateMyDisplayName(string displayName);

    // ================= USERS =================
    User GetUser(Guid userId);
    List<User> SearchUsers(string substr);

    // ================= CONTACTS =================
    User UpdateContactName(Guid userId, string contactName);
    List<User> GetContacts();

    // ================= CHATS =================
    Chat CreatePrivateChat(Guid withUserId);
    Chat CreateGroupChat(string name, List<Guid> memberIds);

    List<Chat> GetAllChats();
    Chat GetChat(Guid chatId);
    List<User> GetChatParticipants(Guid chatId);

    void LeaveChat(Guid chatId);
    void RemoveUserFromChat(Guid chatId, Guid userId);

    // ================= MESSAGES =================
    Message SendMessage(Guid chatId, string text);
    List<Message> GetChatMessages(Guid chatId);

    Message EditMessage(Guid chatId, long messageNum, string newText);
    void DeleteMessage(Guid chatId, long messageNum);

    // ================= READ STATE =================
    void MarkAsRead(Guid chatId, long lastMessageNum);

    // ================= SYNC =================
    void Sync();
}