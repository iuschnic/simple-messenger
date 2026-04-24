using System;
using BL.Models;

namespace BL.Events
{
    public class MessengerEvents
    {
        // Событие для нового сообщения

        public event Func<Message, Task> MessageReceived;
        
        // Событие для выхода пользователя из чата
        public event Func<Guid, Guid, Task> UserLeftChat;
        
        // Событие для создания нового чата
        public event Func<Chat, Task> ChatCreated;

        // Вызов события для получения нового сообщения
        public async Task RaiseMessageReceived(Message message) => await  MessageReceived.Invoke(message);

        // Вызов события для выхода пользователя из чата
        public async Task RaiseUserLeftChat(Guid chatId, Guid userId) => await UserLeftChat.Invoke(chatId, userId);

        // Вызов события для создания нового чата
        public async Task RaiseChatCreated(Chat chat) => await ChatCreated.Invoke(chat);
    }
}