using BL.Models;

namespace BL.Contracts
{
    public interface IRealtimeClient
    {
        // События
        event Action<Message> MessageReceived;
        event Action<Message> MessageUpdated;
        event Action<long> MessageDeleted;
        event Action<Guid, Guid> UserLeftChat;  // Событие для выхода пользователя из чата
        event Action<Guid> ChatCreated;   // Событие для создания нового чата с текущимс пользователем

        // Методы
        // void SendMessage(Message message);
        // void SimulateMessageDeleted(long messageId);
        // void NotifyUserLeftChat(Guid chatId);  // Уведомить клиента о выходе пользователя
        // void NotifyChatCreated(Chat chat);     // Уведомить клиента о создании чата
    }
}