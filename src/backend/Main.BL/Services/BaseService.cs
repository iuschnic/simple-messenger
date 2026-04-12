using Main.BL.Exceptions;
using Main.BL.Models;
using Main.BL.OutPorts;

namespace Main.BL.Services;

public abstract class BaseService
{
    protected readonly IUserRepository _userRepo;
    protected readonly IChatRepository _chatRepo;
    protected readonly IChatUserRepository _chatUserRepo;
    protected readonly IMessageRepository _messageRepo;
    protected BaseService(
        IUserRepository userRepo,
        IChatRepository chatRepo,
        IChatUserRepository chatUserRepo,
        IMessageRepository messageRepo)
    {
        _userRepo = userRepo;
        _chatRepo = chatRepo;
        _chatUserRepo = chatUserRepo;
        _messageRepo = messageRepo;
    }
    protected async Task EnsureUserExists(Guid userId)
    {
        if (!await _userRepo.ExistsAsync(userId))
            throw new NotFoundException(nameof(User), userId);
    }
    protected async Task EnsureChatExists(Guid chatId)
    {
        if (!await _chatRepo.ExistsAsync(chatId))
            throw new NotFoundException(nameof(Chat), chatId);
    }
    protected async Task EnsureParticipant(Guid chatId, Guid userId)
    {
        if (!await _chatUserRepo.IsParticipantAsync(chatId, userId))
            throw new ForbiddenException("User is not a participant of this chat");
    }
    protected async Task EnsureMessageExists(Guid chatId, ulong messageNum)
    {
        if (!await _messageRepo.ExistsAsync(chatId, messageNum))
            throw new NotFoundException(nameof(Message), messageNum);
    }
    protected async Task<Message> GetMessageOrThrow(Guid chatId, ulong messageNum)
    {
        return await _messageRepo.GetByNumberAsync(chatId, messageNum)
            ?? throw new NotFoundException(nameof(Message), messageNum);
    }
    protected async Task<Chat> GetChatOrThrow(Guid chatId)
    {
        return await _chatRepo.GetByIdAsync(chatId)
            ?? throw new NotFoundException(nameof(Chat), chatId);
    }
    protected async Task EnsureNotParticipant(Guid chatId, Guid userId)
    {
        if (await _chatUserRepo.IsParticipantAsync(chatId, userId))
            throw new ConflictException("User is already a participant");
    }
    protected void EnsureOwner(Chat chat, Guid userId)
    {
        if (chat.OwnerUserId != userId)
            throw new ForbiddenException("Only chat owner can perform this action");
    }
    protected void EnsureNotOwner(Chat chat, Guid userId)
    {
        if (chat.OwnerUserId == userId)
            throw new ForbiddenException("Chat owner can not perform this action");
    }
}