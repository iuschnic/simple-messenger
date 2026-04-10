using Main.BL.Models;
using Main.BL.InPorts;

namespace Main.BL.Services;

public class SyncService: ISyncService
{
    public async Task<ChatSync> SyncChatAsync(Guid ChatId, ulong clientVersion, Guid currentUserId)
    {

    }
    public async Task<List<ChatSync>> SyncChatsAsync(List<(Guid, ulong)> Chats, Guid currentUserId)
    {

    }
}
