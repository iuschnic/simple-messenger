using BL.Contracts;
using BL.Models;

using System;

namespace BL.UnitTest.Mocks
{
    public class FakeRealtimeClient : IRealtimeClient
    {
        // События
        public event Func<Message, Task>? MessageReceived;
        public event Func<Message, Task>? MessageUpdated;
        public event Func<ulong, Task>? MessageDeleted;
        public event Func<Guid, Guid, Task>? UserLeftChat;  // Событие для выхода пользователя из чата
        public event Func<Chat, Task>? ChatCreated;   // Событие для создания нового чата с текущим пользователем

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

        public void SimulateMessageDeleted(ulong messageId)
        {
            // Симулируем удаление сообщения
            MessageDeleted?.Invoke(messageId);
        }

        public void NotifyUserLeftChat(Guid chatId, Guid userId)
        {
            // Симулируем выход пользователя из чата
            UserLeftChat?.Invoke(chatId, userId);
        }

        public void NotifyChatCreated(Chat chatId)
        {
            // Симулируем создание нового чата
            ChatCreated?.Invoke(chatId);
        }

        public Task ConnectToHub(string token)
        {
            return Task.CompletedTask;
        }
    }
}