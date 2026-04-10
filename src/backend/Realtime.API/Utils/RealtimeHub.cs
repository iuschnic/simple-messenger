using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Realtime.BL.OutputPorts;
using IGroupManager = Realtime.BL.InputPorts.IGroupManager;
using ILogger = Serilog.ILogger;

namespace Realtime.API.Utils;

public class RealtimeHub(IConnectionManager connectionManager,
    IGroupManager groupManager, HttpChatReceiver chatReceiver, ILogger logger) : Hub
{
    [Authorize]
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            logger.Error("Attempted unauthorized connection");
            throw new HubException("Unauthorized: Please provide valid JWT token");
        }

        await connectionManager.OnUserConnectedAsync(userId, Context.ConnectionId);
        
        var chats = await chatReceiver.GetAllChatsByUserIdAsync(userId);
        
        foreach (var chat in chats)
            await groupManager.AddToGroupAsync(Context.ConnectionId, chat.ToString());
        
        logger.Information("User {UserId} connected", userId);

        await base.OnConnectedAsync().ConfigureAwait(false);
    }
    
    [Authorize]
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            logger.Error("Attempted unauthorized disconnection");
            throw new HubException("Unauthorized: Please provide valid JWT token");
        }

        await connectionManager.OnUserDisconnectedAsync(userId, Context.ConnectionId);
        
        var chats = await chatReceiver.GetAllChatsByUserIdAsync(userId);
        
        foreach (var chat in chats)
            await groupManager.RemoveFromGroupAsync(Context.ConnectionId, chat.ToString());
        
        logger.Information("User {UserId} disconnected", userId);

        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }
}