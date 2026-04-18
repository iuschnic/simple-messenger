using DB.Database;
using DB.Repositories;
using BL.Models;
using System;
using System.IO;

namespace DB.Tests;

public static class IntegrationTests
{
    public static void RunAll()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");

        try
        {
            Console.WriteLine($"==============================");
            Console.WriteLine($"DB Path: {dbPath}");
            Console.WriteLine($"==============================");

            var factory = new DbConnectionFactory(dbPath);

            // 1. init schema
            var init = new DbInitializer(factory);
            init.Init().Wait();

            Console.WriteLine("[INIT] Database schema created");

            // 2. repos
            var auth = new AuthRepository(factory);
            var users = new UserRepository(factory);
            var chats = new ChatRepository(factory);
            var messages = new MessageRepository(factory);

            // =========================
            // AUTH TEST
            // =========================
            Console.WriteLine("\n===== AUTH TEST =====");

            var user = auth.Register("alice", "123", "a@mail.com");
            var authUser = auth.Authenticate("alice", "123");

            Console.WriteLine($"[AUTH] Registered user Id = {user?.Id}");
            Console.WriteLine($"[AUTH] Auth result = {authUser?.UniqueName}");

            Assert(user != null, "Auth.Register failed");
            Assert(authUser.UniqueName == "alice", "Auth.Authenticate failed");

            // =========================
            // USER TEST
            // =========================
            Console.WriteLine("\n===== USER TEST =====");

            var found = users.Find(user.Id);
            Console.WriteLine($"[USER] Find result = {found?.UniqueName}");

            var like = users.FindByUniqueNameLike("ali");
            Console.WriteLine($"[USER] FindByLike count = {like.Count}");

            Assert(found != null, "User.Find failed");
            Assert(like.Count > 0, "FindByLike failed");

            users.UpdateUniqueName(user.Id, "alice2");

            var updated = users.Find(user.Id);
            Console.WriteLine($"[USER] After update = {updated?.UniqueName}");

            Assert(updated.UniqueName == "alice2", "Update failed");

            // =========================
            // CHAT TEST
            // =========================
            Console.WriteLine("\n===== CHAT TEST =====");

            var chat = new Chat
            {
                Id = Guid.NewGuid(),
                OwnerId = user.Id,
                Name = "chat1",
                CreatedAt = DateTime.UtcNow,
                Type = "private",
                Version = 1,
                LastMessageNum = 0
            };

            chats.Save(chat);
            Console.WriteLine($"[CHAT] Chat created Id = {chat.Id}");

            chats.AddUserToChat(chat.Id, user.Id);

            var userChats = chats.GetUserChats(user.Id);
            var chatUsers = chats.FindChatUsers(chat.Id);

            Console.WriteLine($"[CHAT] UserChats count = {userChats.Count}");
            Console.WriteLine($"[CHAT] ChatUsers count = {chatUsers.Count}");

            Assert(userChats.Count == 1, "GetUserChats failed");
            Assert(chatUsers.Count == 1, "FindChatUsers failed");

            // =========================
            // MESSAGE TEST
            // =========================
            Console.WriteLine("\n===== MESSAGE TEST =====");

            var msg = new Message
            {
                ChatId = chat.Id,
                SenderId = user.Id,
                Text = "hello",
                CreatedAt = DateTime.UtcNow,
                Version = 1,
                Type = "regular",
                Deleted = false
            };

            messages.Save(msg);

            Console.WriteLine("[MSG] After Save:");
            Console.WriteLine($"      MessageNumber = {msg.MessageNumber}");
            Console.WriteLine($"      ChatId = {msg.ChatId}");
            Console.WriteLine($"      Text = {msg.Text}");

            var msgs = messages.FindChatMessages(chat.Id);
            Console.WriteLine($"[MSG] Before delete count = {msgs.Count}");

            Assert(msgs.Count == 1, "FindChatMessages failed");

            var edited = messages.Edit(msg.MessageNumber, DateTime.UtcNow, "edited");

            Console.WriteLine($"[MSG] After edit text = {edited.Text}");

            Assert(edited.Text == "edited", "Edit failed");

            Console.WriteLine($"[MSG] Deleting MessageNumber = {msg.MessageNumber}");

            messages.Delete(msg.MessageNumber);

            var afterDelete = messages.FindChatMessages(chat.Id);

            Console.WriteLine($"[MSG] After delete count = {afterDelete.Count}");

            if (afterDelete.Count > 0)
            {
                Console.WriteLine("[MSG] STILL EXISTS:");
                foreach (var m in afterDelete)
                {
                    Console.WriteLine($"   -> {m.MessageNumber} | {m.Text}");
                }
            }

            Assert(afterDelete.Count == 0, "Delete failed");

            Console.WriteLine("\n==============================");
            Console.WriteLine("✅ ALL INTEGRATION TESTS PASSED");
            Console.WriteLine("==============================");
        }
        finally
        {
            Console.WriteLine("\n[CLEANUP] Forcing GC...");

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Thread.Sleep(200);

            try
            {
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                    Console.WriteLine("[CLEANUP] Temp DB deleted");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLEANUP ERROR] {ex.Message}");
            }
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception("TEST FAILED: " + message);
    }
}