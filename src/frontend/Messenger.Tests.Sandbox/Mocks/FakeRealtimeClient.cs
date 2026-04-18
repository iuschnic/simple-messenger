using BL.Contracts;
using BL.Models;
using System;

namespace Messenger.Tests.Sandbox.Mocks
{
    public class FakeRealtimeClient : IRealtimeClient
    {
        // События
        public event Action<Message>? MessageReceived;
        public event Action<Message>? MessageUpdated;
        public event Action<long>? MessageDeleted;
        public event Action<Guid, Guid>? UserLeftChat;  // Событие для выхода пользователя из чата
        public event Action<Guid>? ChatCreated;   // Событие для создания нового чата с текущим пользователем

        // Методы для тестирования

        public void SendMessage(Message message)
        {
            // В реальном клиенте это мог бы быть вызов API или WebSocket
            // Здесь просто симулируем получение сообщения
            MessageReceived?.Invoke(message);
        }

        public void SimulateMessageUpdated(Message message)
        {
            // Симулируем обновление сообщения
            MessageUpdated?.Invoke(message);
        }

        public void SimulateMessageDeleted(long messageId)
        {
            // Симулируем удаление сообщения
            MessageDeleted?.Invoke(messageId);
        }

        public void NotifyUserLeftChat(Guid chatId, Guid userId)
        {
            // Симулируем выход пользователя из чата
            UserLeftChat?.Invoke(chatId, userId);
        }

        public void NotifyChatCreated(Guid chatId)
        {
            // Симулируем создание нового чата
            ChatCreated?.Invoke(chatId);
        }
    }
}