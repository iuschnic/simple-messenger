using Main.BL.Exceptions;
using Main.BL.InPorts;
using Main.BL.Models;
using Main.BL.OutPorts;

namespace Main.BL.Services;

public class SyncService: ISyncService
{
    private readonly IUserRepository _userRepo;
    private readonly IMessageRepository _messageRepo;
    private readonly IChatRepository _chatRepo;
    private readonly IChatUserRepository _chatUserRepo;

    public SyncService(
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
    public async Task<List<ChatSync>> SyncChatsAsync(
        List<(Guid ChatId, ulong ClientVersion)> clientChats,
        Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        var serverChats = await _chatRepo.GetUserChatsAsync(currentUserId);
        var serverChatIds = serverChats.Select(c => c.Id).ToHashSet();

        var clientChatIds = clientChats.Select(c => c.ChatId).ToHashSet();

        var result = new List<ChatSync>();

        // чаты, которые есть и на клиенте, и на сервере (sync)
        var commonChatIds = clientChatIds.Intersect(serverChatIds);
        foreach (var chatId in commonChatIds)
        {
            var clientVersion = clientChats.First(c => c.ChatId == chatId).ClientVersion;
            var sync = await SyncExistingChatAsync(chatId, clientVersion);
            result.Add(sync);
        }

        // новые чаты (есть на сервере, нет на клиенте)
        var newChatIds = serverChatIds.Except(clientChatIds);
        foreach (var chatId in newChatIds)
        {
            var sync = await SyncNewChatAsync(chatId);
            result.Add(sync);
        }

        // удаленные чаты/чаты из которых пользователь вышел (есть на клиенте, нет на сервере)
        var deletedChatIds = clientChatIds.Except(serverChatIds);
        foreach (var chatId in deletedChatIds)
        {
            result.Add(new ChatSync
            {
                Status = ChatSyncStatus.Deleted,
                ChatMeta = new ChatMeta { Id = chatId }
            });
        }

        return result;
    }

    public async Task<ChatSync> SyncChatAsync(
        Guid chatId, ulong clientVersion,
        Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        var chat = await GetChatOrThrow(chatId);
        var participants = await _chatUserRepo.GetChatParticipantsInfosAsync(chatId);
        var updatedMessages = await _messageRepo.GetMessagesAfterVersionAsync(chatId, clientVersion);
        return new ChatSync
        {
            Status = ChatSyncStatus.Synced,
            ChatMeta = new ChatMeta
            {
                Id = chat.Id,
                Name = chat.Name,
                Type = chat.Type,
                OwnerUserId = chat.OwnerUserId,
                CreatedAt = chat.CreatedAt,
                Version = chat.Version,
                LastMessageNum = chat.LastMessageNum
            },
            Messages = [.. updatedMessages],
            Participants = [.. participants]
        };
    }

    private async Task<ChatSync> SyncExistingChatAsync(
        Guid chatId,
        ulong clientVersion)
    {
        var chat = await _chatRepo.GetByIdAsync(chatId);
        var participants = await _chatUserRepo.GetChatParticipantsInfosAsync(chatId);
        var updatedMessages = await _messageRepo.GetMessagesAfterVersionAsync(chatId, clientVersion);

        return new ChatSync
        {
            Status = ChatSyncStatus.Synced,
            ChatMeta = new ChatMeta
            {
                Id = chat.Id,
                Name = chat.Name,
                Type = chat.Type,
                OwnerUserId = chat.OwnerUserId,
                CreatedAt = chat.CreatedAt,
                Version = chat.Version,
                LastMessageNum = chat.LastMessageNum
            },
            Messages = [.. updatedMessages],
            Participants = [.. participants]
        };
    }
    private async Task<ChatSync> SyncNewChatAsync(Guid chatId)
    {
        var chat = await _chatRepo.GetByIdAsync(chatId);
        var participants = await _chatUserRepo.GetChatParticipantsInfosAsync(chatId);
        var lastMessages = await _messageRepo.GetLastMessagesAsync(chatId, 50);
        return new ChatSync
        {
            Status = ChatSyncStatus.New,
            ChatMeta = new ChatMeta
            {
                Id = chat.Id,
                Name = chat.Name,
                Type = chat.Type,
                OwnerUserId = chat.OwnerUserId,
                CreatedAt = chat.CreatedAt,
                Version = chat.Version,
                LastMessageNum = chat.LastMessageNum
            },
            Messages = [.. lastMessages],
            Participants = [.. participants]
        };
    }
    private async Task EnsureUserExists(Guid userId)
    {
        if (!await _userRepo.ExistsAsync(userId))
            throw new NotFoundException(nameof(User), userId);
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
    private async Task EnsureNotParticipant(Guid chatId, Guid userId)
    {
        if (await _chatUserRepo.IsParticipantAsync(chatId, userId))
            throw new ConflictException("User is already a participant");
    }
    private async Task<Chat> GetChatOrThrow(Guid chatId)
    {
        return await _chatRepo.GetByIdAsync(chatId)
            ?? throw new NotFoundException(nameof(Chat), chatId);
    }
    private void EnsureOwner(Chat chat, Guid userId)
    {
        if (chat.OwnerUserId != userId)
            throw new ForbiddenException("Only chat owner can perform this action");
    }
    private void EnsureNotOwner(Chat chat, Guid userId)
    {
        if (chat.OwnerUserId == userId)
            throw new ForbiddenException("Chat owner can not perform this action");
    }
}
