using Main.BL.Enums;

namespace Main.Application.OutPorts;

public interface IMessageProducer
{
    Task SendMessageReceivedAsync(
        ulong messageNumber,
        Guid chatId,
        Guid? senderId,
        string text,
        DateTime createdAt,
        DateTime? editedAt,
        bool isDeleted,
        ulong version,
        MessageType type,
        ulong? replyToMessageNumber,
        Guid? forwardedFromUserId);

    Task SendMessageUpdatedAsync(
        ulong messageNumber,
        Guid chatId,
        Guid? senderId,
        string text,
        DateTime createdAt,
        DateTime? editedAt,
        bool isDeleted,
        ulong version,
        MessageType type,
        ulong? replyToMessageNumber,
        Guid? forwardedFromUserId);

    Task SendMessageReadAsync(
        Guid chatId,
        Guid userId,
        string uniqueName,
        string displayedName,
        ulong lastMessageRead);

    Task SendUserChangedAsync(
        Guid userId,
        string uniqueName,
        string displayedName);

    Task SendChatCreatedAsync(
        Guid chatId,
        string? chatName,
        ChatType chatType,
        Guid? ownerId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum,
        IEnumerable<(Guid userId, string uniqueName, string displayedName)> participants);

    Task SendChatUpdatedAsync(
        Guid chatId,
        string? chatName,
        BL.Enums.ChatType chatType,
        Guid? ownerId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum);

    Task SendChatDeletedAsync(
        Guid chatId,
        string? chatName,
        BL.Enums.ChatType chatType,
        Guid? ownerId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum,
        IEnumerable<(Guid userId, string uniqueName, string displayedName)> participants);

    Task SendChatUserJoinedAsync(
        Guid chatId,
        Guid userId,
        string uniqueName,
        string displayedName,
        ulong lastMessageRead);

    Task SendChatUserLeftAsync(
        Guid chatId,
        Guid userId,
        string uniqueName,
        string displayedName,
        ulong lastMessageRead);
}
