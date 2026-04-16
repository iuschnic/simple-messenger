using Microsoft.AspNetCore.SignalR;
using IGroupManager = Realtime.BL.InputPorts.IGroupManager;

namespace Realtime.API.Utils;

public class GroupManager(IHubContext<RealtimeHub> hubContext, HttpChatReceiver chatReceiver) : IGroupManager
{
    public async Task SendToGroupAsync(string groupName, string eventType, string messageJson) => 
        await hubContext.Clients.Group(groupName).SendAsync(eventType, messageJson);

    public async Task SendToAllUserGroupsAsync(string userId, string eventType, string messageJson)
    {
        var chats = await chatReceiver.GetAllChatsByUserIdAsync(userId);
        
        foreach (var chat in chats)
            await SendToGroupAsync(chat.ToString(), eventType, messageJson);
    }

    public async Task AddToGroupAsync(string connectionId, string groupName) => 
        await hubContext.Groups.AddToGroupAsync(connectionId, groupName);
    
    public async Task RemoveFromGroupAsync(string connectionId, string groupName) => 
        await hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);
}