using Main.Application.Dtos;
using Main.Application.InPorts;
using Main.Application.OutPorts;
using Main.Application.Mappers;

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
                ChatId = chatId
            });
        }

        return result;
    }
    public async Task<ChatSyncDto> SyncChatAsync(
        Guid chatId,
        ulong clientVersion,
        Guid currentUserId)
    {
        await EnsureUserExists(currentUserId);
        var chat = await _chatRepo.GetByIdAsync(chatId);
        if (chat == null || !await _chatUserRepo.IsParticipantAsync(chatId, currentUserId))
        {
            return new ChatSyncDto
            {
                ChatId = chatId,
                Status = ChatSyncStatus.Deleted,
                ChatMeta = null,
                Messages = null,
                Participants = null
            };
        }
        var participants = await _chatUserRepo.GetChatParticipantsInfosAsync(chatId);
        var updatedMessages = await _messageRepo.GetMessagesAfterVersionAsync(chatId, clientVersion);
        return chat.ToDto(ChatSyncStatus.Synced, participants.ToList(), updatedMessages.ToList());
    }

    private async Task<ChatSyncDto> SyncExistingChatAsync(
        Guid chatId,
        ulong clientVersion)
    {
        var chat = await GetChatOrThrow(chatId);
        var participants = await _chatUserRepo.GetChatParticipantsInfosAsync(chatId);
        var updatedMessages = await _messageRepo.GetMessagesAfterVersionAsync(chatId, clientVersion);

        return chat.ToDto(ChatSyncStatus.Synced, [.. participants], [.. updatedMessages]);
    }
    private async Task<ChatSyncDto> SyncNewChatAsync(Guid chatId)
    {
        var chat = await GetChatOrThrow(chatId);
        var participants = await _chatUserRepo.GetChatParticipantsInfosAsync(chatId);
        var lastMessages = await _messageRepo.GetLastMessagesAsync(chatId, 50);

        return chat.ToDto(ChatSyncStatus.New, [.. participants], [.. lastMessages]);
    }
}
