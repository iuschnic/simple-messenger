using Main.BL.Models;

namespace Main.BL.OutPorts;

public interface IChatUserRepository
{
    Task<ChatUser?> GetAsync(Guid chatId, Guid userId);
    Task<IEnumerable<ChatUser>> GetChatParticipantsAsync(Guid chatId);
    Task<ulong> GetLastMessageReadAsync(Guid chatId, Guid userId);
    Task<bool> TryAddAsync(Guid chatId, ChatUser chatUser);
    Task<int> TryAddManyAsync(Guid chatId, IEnumerable<ChatUser> chatUsers);
    Task<bool> TryUpdateLastMessageReadAsync(Guid chatId, Guid userId, ulong lastMessageRead);
    Task<bool> TryRemoveAsync(Guid chatId, Guid userId);
    Task<bool> ExistsAsync(Guid chatId, Guid userId);
}