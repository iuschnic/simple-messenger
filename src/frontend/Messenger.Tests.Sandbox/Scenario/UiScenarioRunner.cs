using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BL.Services;
using BL.Models;
using BL.Contracts;
using BL.Interfaces;

namespace Messenger.Tests.Sandbox.Scenario;

public class UiScenario
{
    private readonly IMessengerService _bl;
    private readonly IHttpClient _http;
    private readonly RepositoryHub _db;

    public UiScenario(IMessengerService bl, IHttpClient http, RepositoryHub db)
    {
        _bl = bl;
        _http = http;
        _db = db;
    }

    public async Task Run()
    {
        Console.WriteLine("=== REGISTRATION FLOW ===");
        
        try
        {
            var alice = await _bl.RegisterUser("alice", "123", "a@mail.com", "yxye");
            Console.WriteLine($"REGISTERED: {alice.Id} {alice.UniqueName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }

        User logged = null;
        if ((await _bl.LoginAgain()) == ReturnCode.Error)
        {
            logged = await _bl.Login("alice", "123");
            Console.WriteLine($"LOGGED IN: {logged.Id} {logged.UniqueName}");
        }
        else
        {
            Console.WriteLine($"LOGGED IN: ERROR");
        }

        var testUser = await _bl.GetCurrentUser();
        Console.WriteLine($"CURRENT USER: {testUser?.Id} {testUser?.UniqueName}");

        testUser = await _bl.UpdateMeDisplayName(testUser!.Id, "alicea");
        Console.WriteLine($"UPDATED USER: {testUser.Id} {testUser.UniqueName}");

        var user1 = await _bl.GetUserByNameWithServer("stass");
        var user2 = await _bl.GetUserByNameWithServer("stasss");
        
        Console.WriteLine($"USER1: {user1.Id} {user1.UniqueName}");
        Console.WriteLine($"USER2: {user2.Id} {user2.UniqueName}");
        
        // user1 = await _bl.UpdateContactName(user1.Id, "fedor");
        // Console.WriteLine($"CONTACT UPDATED: {user1.Id} {user1.ContactName}");

        var contacts = await _bl.FindUsersWithContactName();
        Console.WriteLine($"CONTACTS COUNT: {contacts.Count}");

        var chatPrivate = await _bl.CreatePrivateChat(
            logged.Id,
            new List<Guid> { logged.Id, user1.Id }
        );

        Console.WriteLine($"PRIVATE CHAT: {chatPrivate.Id}");

        var chat = await _bl.CreateGroupChat(
            "TestChat",
            logged.Id,
            new List<Guid> { logged.Id, user1.Id, user2.Id }
        );

        Console.WriteLine($"GROUP CHAT: {chat.Id}");

        var chats = await _bl.GetAllChats();
        Console.WriteLine($"CHATS COUNT: {chats.Count}");

        var participants = await _bl.GetChatParticipants(chat.Id);
        Console.WriteLine($"PARTICIPANTS COUNT: {participants.Count}");

        var f = await _bl.FindUsersByUniqueName(testUser.UniqueName);
        Console.WriteLine($"FOUND: {f?.Id} {f?.UniqueName}");

        var m1 = await _bl.SendMessage(chat.Id, f.Id, "Hello!");
        Console.WriteLine($"m1: {m1.MessageNumber} | {m1.Text}");

        var m2 = await _bl.SendMessage(chat.Id, user1.Id, "Hi Alice!");

        var messagesBefore = await _bl.GetChatMessages(chat.Id);

        await _bl.UpdateLastReadMessageNum(chat.Id, f.Id);

        var m3 = await _bl.SendMessage(chat.Id, user2.Id, "How are you?");

        Console.WriteLine($"m2: {m2.MessageNumber} | {m2.Text}");
        Console.WriteLine($"m3: {m3.MessageNumber} | {m3.Text}");

        var messages = await _bl.GetChatMessages(chat.Id);

        Console.WriteLine($"Messages in DB: {messages.Count}");

        foreach (var m in messages)
        {
            Console.WriteLine($"[{m.MessageNumber}] {m.SenderId}: {m.Text}");
        }

        // await _bl.LeaveChat(chat.Id, f.Id);
    }
}