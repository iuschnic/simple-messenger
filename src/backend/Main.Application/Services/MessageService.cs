using Main.Application.Dtos;
using Main.Application.Exceptions;
using Main.Application.InPorts;
using Main.Application.Mappers;
using Main.Application.OutPorts;
using Main.BL.Models;
using Main.BL.Enums;

namespace Main.Application.Services;

public static class MessagePaging
{
    public const int MaxPageSize = 200;
    public const int MinPageSize = 1;
}

public class MessageService: BaseService, IMessageService
{
    public MessageService(
        IUserRepository userRepo,
        IMessageRepository messageRepo,
        IChatRepository chatRepo,
        IChatUserRepository chatUserRepo,
        IMessageProducer messageProducer) : base(userRepo, chatRepo, chatUserRepo, messageRepo, messageProducer) { }
    public async Task<IEnumerable<MessageDto>> GetOlderMessagesAsync(
        Guid chatId,
        ulong fromMessageNumber,
        int limit,
        Guid currentUserId)
    {
        limit = Math.Clamp(limit, MessagePaging.MinPageSize, MessagePaging.MaxPageSize);
        await EnsureCurrentUserAuthorized(currentUserId);
        await EnsureParticipant(chatId, currentUserId);
        var messages = await _messageRepo.GetOlderMessagesAsync(chatId, fromMessageNumber, limit);
        return messages.Select(m => m.ToDto());
    }
    public async Task<IEnumerable<MessageDto>> GetNewerMessagesAsync(
        Guid chatId,
        ulong fromMessageNumber,
        int limit,
        Guid currentUserId)
    {
        limit = Math.Clamp(limit, MessagePaging.MinPageSize, MessagePaging.MaxPageSize);
        await EnsureCurrentUserAuthorized(currentUserId);
        await EnsureParticipant(chatId, currentUserId);
        var messages = await _messageRepo.GetNewerMessagesAsync(chatId, fromMessageNumber, limit);
        return messages.Select(m => m.ToDto());
    }
    public async Task<IEnumerable<MessageDto>> GetLastMessagesAsync(
        Guid chatId,
        int limit,
        Guid currentUserId)
    {
        limit = Math.Clamp(limit, MessagePaging.MinPageSize, MessagePaging.MaxPageSize);
        await EnsureCurrentUserAuthorized(currentUserId);
        await EnsureParticipant(chatId, currentUserId);
        var messages = await _messageRepo.GetLastMessagesAsync(chatId, limit);
        return messages.Select(m => m.ToDto());
    }

    public async Task CreateRegularMessageAsync(Guid chatId, Guid currentUserId, string text)
    {
        await EnsureCurrentUserAuthorized(currentUserId);
        await EnsureParticipant(chatId, currentUserId);

        var message = Message.CreateRegular(chatId, currentUserId, text);
        await _messageRepo.CreateAsync(message);

        await _messageProducer.SendMessageReceivedAsync(message.MessageNumber, message.ChatId, message.SenderUserId,
            message.Text, message.CreatedAt, message.EditedAt, message.Deleted, message.Version, message.Type,
            message.ReplyToMessageNumber, message.ForwardedFromUserId);
    }

    public async Task CreateReplyMessageAsync(
        Guid chatId, Guid currentUserId, 
        string text, ulong replyToMessageNumber)
    {
        await EnsureCurrentUserAuthorized(currentUserId);
        await EnsureParticipant(chatId, currentUserId);

        var original = await GetMessageOrThrow(chatId, replyToMessageNumber);
        if (original.Deleted)
            throw new RuleViolationException("Cannot reply to a deleted message");

        var message = Message.CreateReply(chatId, currentUserId, text, replyToMessageNumber);
        await _messageRepo.CreateAsync(message);

        await _messageProducer.SendMessageReceivedAsync(message.MessageNumber, message.ChatId, message.SenderUserId,
            message.Text, message.CreatedAt, message.EditedAt, message.Deleted, message.Version, message.Type,
            message.ReplyToMessageNumber, message.ForwardedFromUserId);
    }

    public async Task CreateForwardMessageAsync(
        Guid targetChatId, Guid sourceChatId, 
        ulong sourceMessageNumber, Guid currentUserId)
    {
        await EnsureCurrentUserAuthorized(currentUserId);
        await EnsureParticipant(targetChatId, currentUserId);
        await EnsureParticipant(sourceChatId, currentUserId);

        var original = await GetMessageOrThrow(sourceChatId, sourceMessageNumber);
        if (original.Deleted)
            throw new RuleViolationException("Cannot forward a deleted message");

        var message = Message.CreateForward(
            targetChatId, currentUserId, original.Text, original.SenderUserId);
        await _messageRepo.CreateAsync(message);

        await _messageProducer.SendMessageReceivedAsync(message.MessageNumber, message.ChatId, message.SenderUserId,
            message.Text, message.CreatedAt, message.EditedAt, message.Deleted, message.Version, message.Type,
            message.ReplyToMessageNumber, message.ForwardedFromUserId);
    }

    public async Task EditMessageAsync(
        Guid chatId, ulong messageNumber, 
        string newText, Guid currentUserId)
    {
        await EnsureCurrentUserAuthorized(currentUserId);
        await EnsureParticipant(chatId, currentUserId);

        var message = await GetMessageOrThrow(chatId, messageNumber);
        if (message.SenderUserId != currentUserId)
            throw new ForbiddenException("You can only edit your own message");

        message.EditText(newText);
        await _messageRepo.UpdateAsync(message);

        await _messageProducer.SendMessageUpdatedAsync(message.MessageNumber, message.ChatId, message.SenderUserId,
            message.Text, message.CreatedAt, message.EditedAt, message.Deleted, message.Version, message.Type,
            message.ReplyToMessageNumber, message.ForwardedFromUserId);
    }

    public async Task DeleteMessageAsync(Guid chatId, ulong messageNumber, Guid currentUserId)
    {
        await EnsureCurrentUserAuthorized(currentUserId);
        await EnsureParticipant(chatId, currentUserId);

        var message = await GetMessageOrThrow(chatId, messageNumber);
        if (message.SenderUserId != currentUserId)
            throw new ForbiddenException("You can only delete your own message");

        message.MarkDeleted();
        await _messageRepo.UpdateAsync(message);

        await _messageProducer.SendMessageUpdatedAsync(message.MessageNumber, message.ChatId, message.SenderUserId,
            message.Text, message.CreatedAt, message.EditedAt, message.Deleted, message.Version, message.Type,
            message.ReplyToMessageNumber, message.ForwardedFromUserId);
    }

    public async Task MarkMessagesAsReadAsync(
        Guid chatId,
        ulong lastMessageRead,
        Guid currentUserId)
    {
        var currentUser = await GetCurrentUserOrUnauthorized(currentUserId);
        await EnsureChatExists(chatId);
        await EnsureParticipant(chatId, currentUserId);
        if (!await _chatUserRepo.TryUpdateLastMessageReadAsync(chatId, currentUserId, lastMessageRead))
            throw new TechnicalException("Failed to mark messages read");

        await _messageProducer.SendMessageReadAsync(chatId, currentUser.Id, currentUser.UniqueName,
            currentUser.DisplayedName, lastMessageRead);
    }
}
