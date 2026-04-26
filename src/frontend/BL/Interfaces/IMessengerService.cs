using BL.Models;
using BL.Events;

namespace BL.Interfaces;

public interface IMessengerService
{
    Task<CurrentUser> RegisterUser(string uniqueName, string password, string email, string displayedName);
    Task<User> Login(string u, string p);
    Task<ReturnCode> LoginAgain();

    Task<User?> GetUserById(Guid userId);
    Task<User> UpdateContactName(Guid id, string contact);

    Task<User?> FindUsersByUniqueName(string uniqueName);
    Task<User> GetUserByNameWithServer(string uniqueName);

    Task<List<User>> FindUsersWithContactName();

    Task<List<Chat>> GetAllChats();
    Task<Chat> CreatePrivateChat(Guid creatorId, List<Guid> participants);
    Task<Chat> CreateGroupChat(string name, Guid creatorId, List<Guid> participants);

    Task<List<User>> GetChatParticipants(Guid chatId);
    Task AddUserToChat(Guid chatId, string uniqueName);
    Task LeaveChat(Guid chatId, Guid userId);

    Task<List<Message>> GetChatMessages(Guid chatId);
    Task<Message> SendMessage(Guid chatId, Guid senderId, string text);

    Task<CurrentUser> UpdateMeDisplayName(Guid id, string displayName);
    Task<CurrentUser?> GetCurrentUser();

    Task UpdateLastReadMessageNum(Guid chatId, Guid userId);

    MessengerEvents Events { get; }
    
    Task<User> CreateContact(Guid id, string contact);
}