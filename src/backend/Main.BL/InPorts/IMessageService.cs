using Main.BL.Models;

namespace Main.BL.InPorts;

public interface IMessageService
{
    Task<IEnumerable<Message>> GetMessagesAsync(
        Guid chatId,
        ulong? fromMessageNumber,
        int limit,
        Guid currentUserId);
    Task CreateRegularMessageAsync(
        Guid chatId,
        Guid senderId,
        string text);
    Task CreateReplyMessageAsync(
        Guid chatId,
        Guid senderId,
        string text,
        ulong replyToMessageNumber);
    Task CreateForwardMessageAsync(
        Guid chatId,
        Guid senderId,
        string text,
        Guid forwardedFromUserId);
    Task DeleteMessageAsync(
        Guid chatId,
        ulong messageNum,
        ulong clientVersion,
        Guid currentUserId);
    Task EditMessageAsync(
        Guid chatId,
        ulong messageNum,
        string newText,
        Guid currentUserId);
    Task MarkMessagesAsReadAsync(
        Guid chatId,
        ulong lastMessageRead,
        Guid currentUserId);
}
