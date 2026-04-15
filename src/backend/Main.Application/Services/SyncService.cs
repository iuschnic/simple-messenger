using Main.Application.Dtos;
using Main.Application.InPorts;
using Main.Application.OutPorts;

namespace Main.Application.Services;

public class SyncService: BaseService, ISyncService
{
    public SyncService(
        IUserRepository userRepo,
        IMessageRepository messageRepo,
        IChatRepository chatRepo,
        IChatUserRepository chatUserRepo) : base(userRepo, chatRepo, chatUserRepo, messageRepo) { }
    public async Task<List<ChatSyncDto>> SyncChatsAsync(
        List<(Guid ChatId, ulong ClientVersion)> clientChats,
        Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        var serverChats = await _chatRepo.GetUserChatsAsync(currentUserId);
        var serverChatIds = serverChats.Select(c => c.Id).ToHashSet();

        var clientChatIds = clientChats.Select(c => c.ChatId).ToHashSet();

        var result = new List<ChatSyncDto>();

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
            result.Add(new ChatSyncDto
            {
                Status = ChatSyncStatus.Deleted,
                ChatMeta = new ChatMeta { Id = chatId }
            });
        }

        return result;
    }

    public async Task<ChatSyncDto> SyncChatAsync(
        Guid chatId, ulong clientVersion,
        Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        await EnsureParticipant(chatId, currentUserId);
        var chat = await GetChatOrThrow(chatId);
        var participants = await _chatUserRepo.GetChatParticipantsInfosAsync(chatId);
        var updatedMessages = await _messageRepo.GetMessagesAfterVersionAsync(chatId, clientVersion);
        return new ChatSyncDto
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

    private async Task<ChatSyncDto> SyncExistingChatAsync(
        Guid chatId,
        ulong clientVersion)
    {
        var chat = await _chatRepo.GetByIdAsync(chatId);
        var participants = await _chatUserRepo.GetChatParticipantsInfosAsync(chatId);
        var updatedMessages = await _messageRepo.GetMessagesAfterVersionAsync(chatId, clientVersion);

        return new ChatSyncDto
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
    private async Task<ChatSyncDto> SyncNewChatAsync(Guid chatId)
    {
        var chat = await _chatRepo.GetByIdAsync(chatId);
        var participants = await _chatUserRepo.GetChatParticipantsInfosAsync(chatId);
        var lastMessages = await _messageRepo.GetLastMessagesAsync(chatId, 50);
        return new ChatSyncDto
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
}
