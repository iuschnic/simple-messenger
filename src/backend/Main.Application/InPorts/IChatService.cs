using Main.BL.Models;
namespace Main.Application.InPorts;
public interface IChatService
{
    Task<IEnumerable<Chat>> GetChatsAsync(Guid currentUserId);
    Task<Chat> GetChatByIdAsync(Guid chatId, Guid currentUserId);
    Task<Chat> CreatePrivateChatAsync(Guid otherUserId, Guid currentUserId);
    Task<Chat> CreateGroupChatAsync(string name, List<Guid> memberIds, Guid currentUserId);
    Task UpdateChatNameAsync(Guid chatId, string newName, Guid currentUserId);
    Task AddMemberAsync(Guid chatId, Guid userIdToAdd, Guid currentUserId);
    Task RemoveMemberAsync(Guid chatId, Guid userIdToRemove, Guid currentUserId);
    Task LeaveChatAsync(Guid chatId, Guid currentUserId);
}
