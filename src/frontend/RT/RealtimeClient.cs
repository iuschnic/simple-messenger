using Microsoft.AspNetCore.SignalR.Client;
using BL.Contracts;
using BL.Exceptions;
using BL.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RT.Models;

namespace RT;

public class RealtimeClient(IConfiguration config) : IRealtimeClient, IAsyncDisposable
{
    private HubConnection? _connection;
    private bool _disposed;
    
    public event Func<Message, Task>? MessageReceived;
    public event Func<Message, Task>? MessageUpdated;
    public event Func<ulong, Task>? MessageDeleted;
    public event Func<Guid, Guid, Task>? UserLeftChat;
    public event Func<Chat, Task>? ChatCreated;

    public async Task ConnectToHub(string token)
    {
        if (_connection is not null)
            await DisconnectAsync();
        
        _connection = new HubConnectionBuilder()
            .WithUrl(config["Backend:ApiBaseUrl"] + "/api/v1/hub?access_token=" + token)
            .WithAutomaticReconnect([
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2), 
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(10)
            ])
            .Build();
        
        _connection.On<string>("MessageReceived", OnMessageReceived);
        
        _connection.On<string>("ChatUserLeft", OnUserLeftChat);
        
        _connection.On<string>("ChatCreated", OnChatCreated);

        try
        {
            await _connection.StartAsync().ConfigureAwait(false);
        }
        catch(Exception e)
        {
            await DisposeAsync();
            throw new HubConnectionException(e.Message);
        }
    }

    private async Task OnMessageReceived(string messageJson)
    {
        var messageDto = JsonConvert.DeserializeObject<MessageDto>(messageJson);

        if (messageDto?.SenderId is null)
            return;
        
        var senderId = messageDto.SenderId.Value;

        var message = new Message
        {
            MessageNumber = messageDto.MessageNumber,
            ChatId =  messageDto.ChatId,
            SenderId = senderId,
            Text = messageDto.Text,
            CreatedAt = messageDto.CreatedAt.DateTime,
            EditedAt = messageDto.EditedAt?.DateTime,
            Deleted = messageDto.IsDeleted,
            Version = messageDto.Version,
            Type = messageDto.Type
        };
        
        if  (MessageReceived != null)
            await MessageReceived.Invoke(message);
    }

    private async Task OnUserLeftChat(string chatUserJson)
    {
        var chatUserDto = JsonConvert.DeserializeObject<ChatUserDto>(chatUserJson);
        
        if (chatUserDto is null)
            return;
        
        if (UserLeftChat != null)
            await UserLeftChat.Invoke(chatUserDto.ChatId, chatUserDto.User.Id);
    }

    private async Task OnChatCreated(string chatJson)
    {
        var chatDto = JsonConvert.DeserializeObject<FullChatDto>(chatJson);

        if (chatDto?.Chat.OwnerId is null || chatDto.Chat.Name is null)
            return;
        
        var ownerId = chatDto.Chat.OwnerId.Value;
        
        var users = chatDto.Participants
            .Select(x => x.User)
            .Select(userDto => 
                new User { Id = userDto.Id, UniqueName = userDto.UniqueName, DisplayName = userDto.DisplayedName })
            .ToList();

        var chat = new Chat
        {
            Id = chatDto.Chat.Id,
            OwnerId = ownerId,
            Name = chatDto.Chat.Name ?? string.Empty,
            CreatedAt =  chatDto.Chat.CreatedAt.DateTime,
            Version = chatDto.Chat.Version,
            Type = chatDto.Chat.Type,
            LastMessageNum = chatDto.Chat.LastMessageNum,
            Members = users
        };
        
        if (ChatCreated != null)
            await ChatCreated.Invoke(chat);
    }

    private async Task DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await DisconnectAsync();
            _disposed = true;
        }
    }
}