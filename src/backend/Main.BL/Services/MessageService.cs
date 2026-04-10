using Main.BL.Exceptions;
using Main.BL.InPorts;
using Main.BL.Models;
using Main.BL.OutPorts;

namespace Main.BL.Services;

public class MessageService: IMessageService
{
    private readonly IUserRepository _userRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly IChatRepository _chatRepo;
    private readonly IChatUserRepository _chatUserRepo;

    public MessageService(
        IUserRepository userRepo,
        IMessageRepository messageRepo,
        IChatRepository chatRepo,
        IChatUserRepository chatUserRepo)
    {
        _userRepo = userRepo;
        _messageRepo = messageRepo;
        _chatRepo = chatRepo;
        _chatUserRepo = chatUserRepo;
    }
    public async Task<IEnumerable<Message>> GetOlderMessagesAsync(
        Guid chatId,
        ulong fromMessageNumber,
        int limit,
        Guid currentUserId)
    {
        if (limit <= 0)
            throw new ArgumentException("limit must be positive");
        limit = Math.Min(limit, 200);
        await EnsureUserExists(currentUserId);
        await EnsureChatExists(chatId);
        await EnsureParticipant(chatId, currentUserId);
        return await _messageRepo.GetOlderMessagesAsync(chatId, fromMessageNumber, limit);
    }
    public async Task<IEnumerable<Message>> GetNewerMessagesAsync(
        Guid chatId,
        ulong fromMessageNumber,
        int limit,
        Guid currentUserId)
    {
        if (limit <= 0)
            throw new ArgumentException("limit must be positive");
        limit = Math.Min(limit, 200);
        await EnsureUserExists(currentUserId);
        await EnsureChatExists(chatId);
        await EnsureParticipant(chatId, currentUserId);
        return await _messageRepo.GetNewerMessagesAsync(chatId, fromMessageNumber, limit);
    }
    public async Task<IEnumerable<Message>> GetLastMessagesAsync(
        Guid chatId,
        int limit,
        Guid currentUserId)
    {
        if (limit <= 0)
            throw new ArgumentException("limit must be positive");
        limit = Math.Min(limit, 200);
        await EnsureUserExists(currentUserId);
        await EnsureChatExists(chatId);
        await EnsureParticipant(chatId, currentUserId);
        return await _messageRepo.GetLastMessagesAsync(chatId, limit);
    }
    public async Task CreateRegularMessageAsync(
        Guid chatId,
        Guid currentUserId,
        string text)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Message text should not be null/whitespace");
        await EnsureUserExists(currentUserId);
        await EnsureChatExists(chatId);
        await EnsureParticipant(chatId, currentUserId);
        if (!await _messageRepo.TryCreateAsync(Message.CreateRegular(chatId, currentUserId, text)))
            throw new TechnicalException("Failed to create message");

    }
    public async Task CreateReplyMessageAsync(
        Guid chatId,
        Guid currentUserId,
        string text,
        ulong replyToMessageNumber)
    {
        if (string.IsNullOrEmpty(text))
            throw new ArgumentException("Message text should not be null/whitespace");
        await EnsureUserExists(currentUserId);
        await EnsureChatExists(chatId);
        await EnsureParticipant(chatId, currentUserId);
        await EnsureMessageExists(chatId, replyToMessageNumber);

        var original = await GetMessageOrThrow(chatId, replyToMessageNumber);
        if (original.Deleted)
            throw new RuleViolationException("Cannot reply to a deleted message");

        if (!await _messageRepo.TryCreateAsync(Message.CreateReply(chatId, currentUserId, text, replyToMessageNumber)))
            throw new TechnicalException("Failed to create message");
    }
    public async Task CreateForwardMessageAsync(
        Guid targetChatId,
        Guid sourceChatId,
        ulong sourceMessageNumber,
        Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        await EnsureChatExists(targetChatId);
        await EnsureChatExists(sourceChatId);
        await EnsureParticipant(targetChatId, currentUserId);
        await EnsureParticipant(sourceChatId, currentUserId);

        var original = await GetMessageOrThrow(sourceChatId, sourceMessageNumber);
        if (original.Deleted)
            throw new RuleViolationException("Cannot forward a deleted message");

        if (!await _messageRepo.TryCreateAsync(Message.CreateForward(targetChatId, currentUserId,
            original.Text, original.SenderUserId)))
            throw new TechnicalException("Failed to create message");
    }
    public async Task DeleteMessageAsync(
        Guid chatId,
        ulong messageNumber,
        Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        await EnsureChatExists(chatId);
        var message = await GetMessageOrThrow(chatId, messageNumber);
        if (message.Deleted)
            throw new RuleViolationException("Cannot delete a deleted message");
        if (!await _messageRepo.TryDeleteAsync(chatId, messageNumber))
            throw new TechnicalException("Failed to delete message");
    }
    public async Task EditMessageAsync(
        Guid chatId,
        ulong messageNumber,
        string newText,
        Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        await EnsureChatExists(chatId);
        var message = await GetMessageOrThrow(chatId, messageNumber);
        if (message.Deleted)
            throw new RuleViolationException("Cannot delete a deleted message");
        if (!await _messageRepo.TryEditTextAsync(chatId, messageNumber, newText))
            throw new TechnicalException("Failed to edit message");
    }
    public async Task MarkMessagesAsReadAsync(
        Guid chatId,
        ulong lastMessageRead,
        Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        await EnsureChatExists(chatId);
        if (!await _chatUserRepo.TryUpdateLastMessageReadAsync(chatId, currentUserId, lastMessageRead))
            throw new TechnicalException("Failed to mark messages read");
    }

    private async Task EnsureUserExists(Guid userId)
    {
        if (!await _userRepo.ExistsAsync(userId))
            throw new NotFoundException(nameof(User), userId);
    }
    private async Task EnsureMessageExists(Guid chatId, ulong messageNum)
    {
        if (!await _messageRepo.ExistsAsync(chatId, messageNum))
            throw new NotFoundException(nameof(Message), messageNum);
    }
    private async Task<Message> GetMessageOrThrow(Guid chatId, ulong messageNum)
    {
        return await _messageRepo.GetByNumberAsync(chatId, messageNum)
            ?? throw new NotFoundException(nameof(Message), messageNum);
    }
    private async Task EnsureChatExists(Guid chatId)
    {
        if (!await _chatRepo.ExistsAsync(chatId))
            throw new NotFoundException(nameof(Chat), chatId);
    }
    private async Task EnsureParticipant(Guid chatId, Guid userId)
    {
        if (!await _chatUserRepo.IsParticipantAsync(chatId, userId))
            throw new ForbiddenException("User is not a participant of this chat");
    }
}
