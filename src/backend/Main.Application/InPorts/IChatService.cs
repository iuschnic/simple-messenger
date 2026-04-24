using Main.Application.Dtos;
namespace Main.Application.InPorts;
public interface IChatService
{
    Task<IEnumerable<ChatWithUsersDto>> GetChatsAsync(Guid currentUserId);
    Task<ChatWithUsersDto> GetChatByIdAsync(Guid chatId, Guid currentUserId);
    Task<Guid> CreatePrivateChatAsync(Guid otherUserId, Guid currentUserId);
    Task<Guid> CreateGroupChatAsync(string name, List<Guid> memberIds, Guid currentUserId);
    Task UpdateChatNameAsync(Guid chatId, string newName, Guid currentUserId);
    Task AddMemberAsync(Guid chatId, Guid userIdToAdd, Guid currentUserId);
    Task RemoveMemberAsync(Guid chatId, Guid userIdToRemove, Guid currentUserId);
    Task LeaveChatAsync(Guid chatId, Guid currentUserId);
}
