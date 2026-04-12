using Main.Application.Dtos;
namespace Main.Application.InPorts;
public interface ISyncService
{
    Task<ChatSync> SyncChatAsync(Guid chatId, ulong clientVersion, Guid currentUserId);
    Task<List<ChatSync>> SyncChatsAsync(List<(Guid ChatId, ulong ClientVersion)> clientChats, Guid currentUserId);
}
