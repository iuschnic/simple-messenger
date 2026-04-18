using System;
using System.Collections.Generic;
using BL;
using BL.Services;
using BL.Models;
using BL.Contracts;
using BL.Interfaces;

namespace Messenger.Tests.Sandbox.Scenario;

public class UiScenario
{

    private readonly MessengerService _bl;
    private readonly IHttpClient _http;
    private readonly RepositoryHub _db;
    private CurrentUser currentUser;

    public UiScenario(MessengerService bl, IHttpClient http, RepositoryHub db)
    {
        _bl = bl;
        _http = http;
        _db = db;
    }

    public void Run()
    {
        Console.WriteLine("=== REGISTRATION FLOW ===");

        // 1. регистрация 11
        try
        {
            var alice = _bl.RegisterUser("alice", "123", "a@mail.com", "yxye");
            Console.WriteLine($"REGISTERED: {alice.Id} {alice.UniqueName}");
            currentUser = alice;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }

        //авторизация 11
        var logged = _bl.Login("alice", "123");
        Console.WriteLine($"LOGGED IN: {logged.Id} {logged.UniqueName}");


        //отображение информации о пользователе 11
        var test_user = _bl.GetCurrentUser();
        Console.WriteLine($"CURRENT USER: {test_user.Id} {test_user.UniqueName}");
        
        //обновление имени 11
        currentUser = _bl.UpdateMeDisplayName(currentUser.Id, "alicea");
        Console.WriteLine($"CURRENT USER: {currentUser.Id} {currentUser.UniqueName}");

        //находим пользователей (подтягиваем в Бд) 11
        User user1 = _bl.GetUserByNameWithServer("stass");
        User user2 = _bl.GetUserByNameWithServer("stasss");
        Console.WriteLine($"USER1 IN: {user1.Id} {user1.ContactName}");
        Console.WriteLine($"USER2 IN: {user2.Id} {user2.UniqueName}");

        //добавляем в контакты 11
        user1 = _bl.UpdateContactName(user1.Id, "fedor");
        Console.WriteLine($"LOGGED IN: {user1.Id} {user1.ContactName}");

        //cмотрим список контактов 11
        List<User> contacts_list = _bl.FindUsersWithContactName();
        Console.WriteLine($"contacts cnt: {contacts_list.Count}");
        //Guid currentuser_id = _bl.GetUserByNameWithServer("alicea").Id;

        //создаём приватный чат
        var chat_private = _bl.CreatePrivateChat(
            logged.Id,
            new List<Guid> { logged.Id, user1.Id }
        );
        Console.WriteLine($"Chat id: {chat_private.Id} ");
        
        //создаём чат
        var chat = _bl.CreateGroupChat(
            "TestChat",
            logged.Id,
            new List<Guid> {logged.Id, user1.Id, user2.Id }
        );
        Console.WriteLine($"Chat id: {chat.Id} ");
        
        //получение списка чатов 11
        List<Chat> chats_list = _bl.GetAllChats();
        Console.WriteLine($"chats_list cnt: {chats_list.Count}");
        
        //получение участников чата 11
        List<User> participants_list = _bl.GetChatParticipants(chat.Id);
        Console.WriteLine($"participants_list cnt: {participants_list.Count}");
        Console.WriteLine($"participants_list cnt: {participants_list.First().UniqueName}");
        
        //найти пользователя по имени у себя в чатах 11
        var f = _bl.FindUsersByUniqueName(currentUser.UniqueName);
        Console.WriteLine($"F: {f.Id} {f.UniqueName}");
        
        //отправка сообщения
        var m1 = _bl.SendMessage(chat.Id, f.Id, "Hello!");
        Console.WriteLine($"m1: {m1.MessageNumber} | {m1.Text}");
        var m2 = _bl.SendMessage(chat.Id, user1.Id, "Hi Alice!");
        var messagess = _bl.GetChatMessages(chat.Id);
        
        //при выходе из чата нажимается (помечается последнее сообщение в чате) 11
        _bl.UpdateLastReadMessageNum(chat.Id, f.Id);
        
        var m3 = _bl.SendMessage(chat.Id, user2.Id, "How are you?");
        
        Console.WriteLine($"m2: {m2.MessageNumber} | {m2.Text}");
        Console.WriteLine($"m3: {m3.MessageNumber} | {m3.Text}");

        _db.Messages.Save(new Message
        {
            MessageNumber = 4,
            ChatId = chat.Id,
            SenderId = user1.Id,
            Text = "Hi",
            CreatedAt = DateTime.UtcNow,
            Version = 3,
            Type = "regular"
        });
        //получить список сообщений из чата 11
        var messages = _bl.GetChatMessages(chat.Id);
        
        Console.WriteLine($"Messages in DB: {messages.Count}");
        
        
        foreach (var m in messages)
        {
            Console.WriteLine($"[{m.MessageNumber}] {m.SenderId}: {m.Text}");
        }
        
        //выйти из чата 11
        _bl.LeaveChat(chat.Id, f.Id);

        
        
        
        // Console.WriteLine("====================================");
        // Console.WriteLine("UI SCENARIO START");
        // Console.WriteLine("====================================");
        //
        // // ================= USERS =================
        // Console.WriteLine("\n=== REGISTER USERS ===");
        //
        // var alice = _bl.RegisterUser("alice", "123", "a@mail.com");
        // var bob = _bl.RegisterUser("bob", "123", "b@mail.com");
        //
        // Console.WriteLine($"Alice: {alice.Id} {alice.UniqueName}");
        // Console.WriteLine($"Bob:   {bob.Id} {bob.UniqueName}");
        //
        // // ================= CHAT =================
        // Console.WriteLine("==н= REGISTER USERS ===");
        //
        // var user = _bl.RegisterUser("alice", "123", "a@mail.com");
        //
        // Console.WriteLine($"User: {user.Id}");
        //
        // Console.WriteLine("=== CREATE CHAT ===");
        //
        // var chat = _bl.CreateChat(
        //     "Test chat",
        //     user.Id, // ✅ ВАЖНО — тот же ID
        //     new List<string>()
        // );
        //
        // // ================= SEND MESSAGES =================
        // Console.WriteLine("\n=== SEND MESSAGES ===");
        //
        // var m1 = _bl.SendMessage(chat.Id, alice.Id, "Hello Bob!");
        // var m2 = _bl.SendMessage(chat.Id, bob.Id, "Hi Alice!");
        // var m3 = _bl.SendMessage(chat.Id, alice.Id, "How are you?");
        //
        // Console.WriteLine($"m1: {m1.MessageNumber} | {m1.Text}");
        // Console.WriteLine($"m2: {m2.MessageNumber} | {m2.Text}");
        // Console.WriteLine($"m3: {m3.MessageNumber} | {m3.Text}");
        //
        // // ================= LOAD CHAT HISTORY =================
        // Console.WriteLine("\n=== LOAD CHAT HISTORY ===");
        //
        // var messages = _bl.GetChatMessages(chat.Id);
        //
        // Console.WriteLine($"Messages in DB: {messages.Count}");
        //
        // foreach (var m in messages)
        // {
        //     Console.WriteLine($"[{m.MessageNumber}] {m.SenderId}: {m.Text}");
        // }
        //
        // // ================= EDIT MESSAGE =================
        // Console.WriteLine("\n=== EDIT MESSAGE ===");
        //
        // var edited = _bl.EditMessage(m2.MessageNumber, "Hi Alice!!! (edited)");
        //
        // Console.WriteLine($"Edited: {edited.MessageNumber} | {edited.Text}");
        //
        // // ================= DELETE MESSAGE =================
        // Console.WriteLine("\n=== DELETE MESSAGE ===");
        //
        // _bl.DeleteMessage(m1.MessageNumber);
        //
        // Console.WriteLine($"Deleted message {m1.MessageNumber}");
        //
        // // ================= FINAL STATE =================
        // Console.WriteLine("\n=== FINAL STATE ===");
        //
        // var finalMessages = _bl.GetChatMessages(chat.Id);
        //
        // foreach (var m in finalMessages)
        // {
        //     Console.WriteLine($"[{m.MessageNumber}] {m.Text}");
        // }
        //
        //         // ================= EXTRA SCENARIO: 3 PEOPLE CHAT =================
        // Console.WriteLine("\n\n====================================");
        // Console.WriteLine("EXTRA SCENARIO: 3 PEOPLE CHAT");
        // Console.WriteLine("====================================");
        //
        // var alice2 = _bl.RegisterUser("alice2", "123", "a2@mail.com");
        // var bob2   = _bl.RegisterUser("bob2", "123", "b2@mail.com");
        // var carl2  = _bl.RegisterUser("carl2", "123", "c2@mail.com");
        //
        // Console.WriteLine($"A2: {alice2.Id}");
        // Console.WriteLine($"B2: {bob2.Id}");
        // Console.WriteLine($"C2: {carl2.Id}");
        //
        // var groupChat = _bl.CreateChat(
        //     "3-person chat",
        //     alice2.Id,
        //     new List<string> { "bob2", "carl2" }
        // );
        //
        // Console.WriteLine($"Group chat: {groupChat.Id}");
        //
        // // ===== MESSAGES FLOW =====
        // Console.WriteLine("\n--- MESSAGE FLOW ---");
        //
        // var gm1 = _bl.SendMessage(groupChat.Id, alice2.Id, "Hey team!");
        // var gm2 = _bl.SendMessage(groupChat.Id, bob2.Id,   "Hi Alice & Carl");
        // var gm3 = _bl.SendMessage(groupChat.Id, carl2.Id,  "Hello everyone");
        // var gm4 = _bl.SendMessage(groupChat.Id, bob2.Id,   "What are we doing?");
        //
        // // ===== PRINT RAW =====
        // Console.WriteLine("\n--- RAW MESSAGES ---");
        //
        // var groupMessages = _bl.GetChatMessages(groupChat.Id);
        //
        // foreach (var m in groupMessages)
        // {
        //     Console.WriteLine($"[{m.MessageNumber}] {m.SenderId}: {m.Text}");
        // }
        //
        // // ===== MAP NAMES =====
        // var map = new Dictionary<Guid, string>
        // {
        //     { alice2.Id, "Alice2" },
        //     { bob2.Id,   "Bob2" },
        //     { carl2.Id,  "Carl2" }
        // };
        //
        // Console.WriteLine("\n--- HUMAN READABLE ---");
        //
        // foreach (var m in groupMessages)
        // {
        //     var name = map.ContainsKey(m.SenderId)
        //         ? map[m.SenderId]
        //         : m.SenderId.ToString();
        //
        //     Console.WriteLine($"{name} → {m.Text}");
        // }
        //
        // // ===== EDIT + DELETE =====
        // Console.WriteLine("\n--- EDIT MESSAGE ---");
        //
        // var edited2 = _bl.EditMessage(gm2.MessageNumber, "Hi all (edited in group)");
        // Console.WriteLine($"Edited: {edited2.MessageNumber} | {edited2.Text}");
        //
        // Console.WriteLine("\n--- DELETE MESSAGE ---");
        //
        // _bl.DeleteMessage(gm1.MessageNumber);
        // Console.WriteLine($"Deleted: {gm1.MessageNumber}");
        //
        // // ===== FINAL =====
        // Console.WriteLine("\n--- FINAL GROUP STATE ---");
        //
        // var finalGroup = _bl.GetChatMessages(groupChat.Id);
        //
        // foreach (var m in finalGroup)
        // {
        //     var name = map.ContainsKey(m.SenderId)
        //         ? map[m.SenderId]
        //         : "unknown";
        //
        //     Console.WriteLine($"[{m.MessageNumber}] {name}: {m.Text}");
        // }
        //
        // Console.WriteLine("\n====================================");
        // Console.WriteLine("UI SCENARIO END");
        // Console.WriteLine("==================
    }
}