using Main.Application.Dtos;
namespace Main.Application.InPorts;
public interface ISyncService
{
    Task<ChatSyncDto> SyncChatAsync(Guid chatId, ulong clientVersion, Guid currentUserId);
    Task<List<ChatSyncDto>> SyncChatsAsync(List<(Guid ChatId, ulong ClientVersion)> clientChats, Guid currentUserId);
}
