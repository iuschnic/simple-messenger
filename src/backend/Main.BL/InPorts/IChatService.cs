using Main.BL.Models;

namespace Main.BL.InPorts;

public interface IChatService
{
    Task<IEnumerable<Chat>> GetMyChatsAsync(Guid currentUserId);
    Task<Chat> GetChatByIdAsync(Guid chatId, Guid currentUserId);
    Task<Chat> CreatePrivateChatAsync(Guid member1, Guid member2);
    Task<Chat> CreateGroupChatAsync(string name, List<Guid> memberIds, Guid currentUserId);
    Task AddMemberAsync(Guid chatId, Guid userIdToAdd, Guid currentUserId);
    Task RemoveMemberAsync(Guid chatId, Guid userIdToRemove, Guid currentUserId);
    Task LeaveChatAsync(Guid chatId, Guid currentUserId);
}
