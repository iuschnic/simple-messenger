using BL.Contracts;
using BL.Models;
using MessageModel = BL.Models.Message;

namespace UI;

internal sealed class FakeRealtimeClient : IRealtimeClient
{
    public event Func<MessageModel, Task>? MessageReceived;
    public event Func<MessageModel, Task>? MessageUpdated;
    public event Func<ulong, Task>? MessageDeleted;
    public event Func<Guid, Guid, Task>? UserLeftChat;
    public event Func<Chat, Task>? ChatCreated;
    public event Func<Task>? ReconnectedToHub;

    public Task ConnectToHub(string token)
        => Task.CompletedTask;

    public Task EmitMessageReceived(MessageModel message)
        => MessageReceived?.Invoke(message) ?? Task.CompletedTask;

    public Task EmitUserLeftChat(Guid chatId, Guid userId)
        => UserLeftChat?.Invoke(chatId, userId) ?? Task.CompletedTask;

    public Task EmitChatCreated(Chat chat)
        => ChatCreated?.Invoke(chat) ?? Task.CompletedTask;

    public Task EmitReconnected()
        => ReconnectedToHub?.Invoke() ?? Task.CompletedTask;
}
