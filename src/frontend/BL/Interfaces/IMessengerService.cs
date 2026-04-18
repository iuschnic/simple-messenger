using BL.Models;
using BL.Events;

namespace BL.Interfaces;

public interface IMessengerService
{
    CurrentUser RegisterUser(string uniqueName, string password, string email, string displayedName);
    User Login(string u, string p);

    User GetUserById(Guid userId);
    public User UpdateContactName(Guid id, string contact);
    
    public User FindUsersByUniqueName(string uniqueName);

    List<Chat> GetAllChats();
    public Chat CreatePrivateChat(Guid creatorId, List<Guid> participants);

    List<User> GetChatParticipants(Guid chatId);
    void AddUserToChat(Guid chatId, string uniqueName);
    void LeaveChat(Guid chatId, Guid userId);
    public Chat CreateGroupChat(string name, Guid creatorId, List<Guid> participants);

    List<Message> GetChatMessages(Guid chatId);
    public CurrentUser UpdateMeDisplayName(Guid id, string displayName);

    Message SendMessage(Guid chatId, Guid senderId, string text);
    // Message EditMessage(long messageId, string newText);
    // void DeleteMessage(long messageId);

    MessengerEvents Events { get; }
    public CurrentUser GetCurrentUser();
    public void UpdateLastReadMessageNum(Guid chatId, Guid userId);
}