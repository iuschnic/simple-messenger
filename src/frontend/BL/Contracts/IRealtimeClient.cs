using BL.Models;

namespace BL.Contracts
{
    public interface IRealtimeClient
    {
        event Func<Message, Task> MessageReceived;
        event Func<Message, Task> MessageUpdated;
        event Func<ulong, Task> MessageDeleted;
        event Func<Guid, Guid, Task> UserLeftChat;  // Событие для выхода пользователя из чата
        event Func<Chat, Task> ChatCreated;   // Событие для создания нового чата с текущимс пользователем
        
        event Func<Task> ReconnectedToHub;
            
        public Task ConnectToHub(string token);
        // Методы
        // void SendMessage(Message message);
        // void SimulateMessageDeleted(long messageId);
        // void NotifyUserLeftChat(Guid chatId);  // Уведомить клиента о выходе пользователя
        // void NotifyChatCreated(Chat chat);     // Уведомить клиента о создании чата
    }
}