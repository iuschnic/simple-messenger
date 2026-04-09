using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Realtime.BL.OutputPorts;
using IGroupManager = Realtime.BL.InputPorts.IGroupManager;

namespace Realtime.API.Utils;

public class RealtimeHub(IConnectionManager connectionManager, IGroupManager groupManager, HttpChatReceiver chatReceiver) : Hub
{
    [Authorize]
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userId is null)
            return;

        await connectionManager.OnUserConnectedAsync(userId, Context.ConnectionId);
        
        var chats = await chatReceiver.GetAllChatsByUserIdAsync(userId);
        
        foreach (var chat in chats)
            await groupManager.AddToGroupAsync(Context.ConnectionId, chat.ToString());

        await base.OnConnectedAsync().ConfigureAwait(false);
    }
    
    [Authorize]
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userId is null)
            return;

        await connectionManager.OnUserDisconnectedAsync(userId, Context.ConnectionId);
        
        var chats = await chatReceiver.GetAllChatsByUserIdAsync(userId);
        
        foreach (var chat in chats)
            await groupManager.RemoveFromGroupAsync(Context.ConnectionId, chat.ToString());

        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }
}