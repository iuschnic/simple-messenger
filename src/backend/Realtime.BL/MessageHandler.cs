using Newtonsoft.Json;
using Realtime.BL.InputPorts;
using Realtime.BL.OutputPorts;
using Serilog;
using Shared.Main.Realtime;
using Shared.Main.Realtime.Models;

namespace Realtime.BL;

public class MessageHandler(IConnectionsRepository repository,
    IGroupManager groupManager, ILogger logger) : IMessageHandler
{
    public async Task OnMessageReceivedFromBrokerAsync(EventType eventType, string dataJson)
    {
        logger.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson);
        switch (eventType)
        {
            case EventType.MessageReceived or EventType.MessageUpdated:
                await MessageReceivedOrUpdated(eventType, dataJson);
                break;
            case EventType.UserChanged:
                await UserChanged(dataJson);
                break;
            case EventType.MessageRead or EventType.ChatUserJoined or EventType.ChatUserLeft:
                await MessageReadOrUserJoinedOrLeft(eventType, dataJson);
                break;
            case EventType.ChatUpdated:
                await ChatUpdated(dataJson);
                break;
            case EventType.ChatCreated:
                await ChatCreated(dataJson);
                break;
        }
    }

    private async Task MessageReceivedOrUpdated(EventType eventType, string dataJson)
    {
        var message = JsonConvert.DeserializeObject<MessageDto>(dataJson);
                
        if (message is null)
            throw new FailedToDeserializeMessageException();
                
        await groupManager.SendToGroupAsync(message.ChatId.ToString(), eventType.ToString(), dataJson);
    }

    private async Task UserChanged(string dataJson)
    {
        var user = JsonConvert.DeserializeObject<UserDto>(dataJson);
                
        if (user is null)
            throw new FailedToDeserializeMessageException();
                
        await groupManager.SendToAllUserGroupsAsync(user.Id.ToString(), nameof(EventType.UserChanged), dataJson);
    }

    private async Task MessageReadOrUserJoinedOrLeft(EventType eventType, string dataJson)
    {
        var chatUser = JsonConvert.DeserializeObject<ChatUserDto>(dataJson);
                
        if (chatUser is null)
            throw new FailedToDeserializeMessageException();
                
        var connections = repository.GetConnectionIdsByUserId(chatUser.User.Id.ToString());
        foreach (var connectionId in connections)
            if (eventType is EventType.ChatUserJoined)
                await groupManager.AddToGroupAsync(connectionId, chatUser.ChatId.ToString());
            else
                await groupManager.RemoveFromGroupAsync(connectionId, chatUser.ChatId.ToString());
                
        await groupManager.SendToGroupAsync(chatUser.ChatId.ToString(), eventType.ToString(), dataJson);
    }

    private async Task ChatUpdated(string dataJson)
    {
        var chat = JsonConvert.DeserializeObject<ChatDto>(dataJson);
                
        if (chat is null)
            throw new FailedToDeserializeMessageException();
                
        await groupManager.SendToGroupAsync(chat.Id.ToString(), nameof(EventType.ChatUpdated), dataJson);
    }
    
    private async Task ChatCreated(string dataJson)
    {
        var newChat = JsonConvert.DeserializeObject<NewChatDto>(dataJson);
                
        if (newChat is null)
            throw new FailedToDeserializeMessageException();

        foreach (var connectionId in newChat.Participants.Select(participant => 
                         repository.GetConnectionIdsByUserId(participant.User.Id.ToString()))
                     .SelectMany(x => x))
            await groupManager.AddToGroupAsync(connectionId, newChat.Chat.Id.ToString());
                
        await groupManager.SendToGroupAsync(newChat.Chat.Id.ToString(), nameof(EventType.ChatCreated), dataJson);
    }
}