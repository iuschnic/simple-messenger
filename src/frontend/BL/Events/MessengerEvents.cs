using System;
using BL.Models;

namespace BL.Events
{
    public class MessengerEvents
    {
        // Событие для нового сообщения
        public event Action<Message> MessageReceived;
        
        // Событие для выхода пользователя из чата
        public event Action<Guid, Guid> UserLeftChat;
        
        // Событие для создания нового чата
        public event Action<Guid> ChatCreated;

        // Вызов события для получения нового сообщения
        public void RaiseMessageReceived(Message message) => MessageReceived?.Invoke(message);

        // Вызов события для выхода пользователя из чата
        public void RaiseUserLeftChat(Guid chatId, Guid userId) => UserLeftChat?.Invoke(chatId, userId);

        // Вызов события для создания нового чата
        public void RaiseChatCreated(Guid chat) => ChatCreated?.Invoke(chat);
    }
}