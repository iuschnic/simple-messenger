using Main.Application.Dtos;
using Main.BL.Models;

namespace Main.Application.OutPorts;

public interface IChatUserRepository
{
    Task<ChatUser?> GetAsync(Guid chatId, Guid userId);
    Task<IEnumerable<ChatUser>> GetChatParticipantsIdsAsync(Guid chatId);
    Task<IEnumerable<User>> GetChatParticipantsAsync(Guid chatId);
    Task<IEnumerable<ChatParticipantInfo>> GetChatParticipantsInfosAsync(Guid chatId);
    Task<ulong> GetLastMessageReadAsync(Guid chatId, Guid userId);
    Task<bool> TryAddAsync(Guid chatId, ChatUser chatUser);
    Task<int> TryAddManyAsync(Guid chatId, IEnumerable<ChatUser> chatUsers);
    Task<bool> TryUpdateLastMessageReadAsync(Guid chatId, Guid userId, ulong lastMessageRead);
    Task<bool> TryRemoveAsync(Guid chatId, Guid userId);
    Task<bool> IsParticipantAsync(Guid chatId, Guid userId);
}