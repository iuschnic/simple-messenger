using BL.Models;
using BL.Services;
using BL.UnitTest.Mocks;
using Xunit;

namespace BL.UnitTest;

public class MessengerServiceTests
{
    private MessengerService CreateService(out FakeRepositoryHub db, out FakeRealtimeClient rt, out FakeHttpClient http)
    {
        http = new FakeHttpClient();
        rt = new FakeRealtimeClient();
        db = new FakeRepositoryHub();

        return new MessengerService(http, rt, db);
    }

    // ================= AUTH =================

    [Fact]
    public void RegisterUser_ShouldSaveCurrentUser()
    {
        var bl = CreateService(out var db, out _, out _);

        var user = bl.RegisterUser("alice", "123", "a@mail.com", "Alice");

        var stored = db.CurrentUser.Get();

        Assert.Equal(user.UniqueName, stored.UniqueName);
    }

    [Fact]
    public void Login_ShouldThrow_WhenWrongCredentials()
    {
        var bl = CreateService(out var db, out _, out _);

        db.CurrentUser.Save(new CurrentUser
        {
            UniqueName = "alice",
            PasswordHash = "123"
        });

        Assert.Throws<Exception>(() => bl.Login("alice", "wrong"));
    }

    [Fact]
    public void Login_ShouldSaveUser()
    {
        var bl = CreateService(out var db, out _, out _);

        db.CurrentUser.Save(new CurrentUser
        {
            UniqueName = "alice",
            PasswordHash = "123"
        });

        var user = bl.Login("alice", "123");

        Assert.NotNull(db.Users.Find(user.Id));
    }

    // ================= USERS =================

    [Fact]
    public void UpdateMeDisplayName_ShouldUpdateLocalUser()
    {
        var bl = CreateService(out var db, out _, out _);

        var cu = db.CurrentUser.Save(new CurrentUser
        {
            UniqueName = "alice",
            DisplayedName = "old"
        });

        bl.UpdateMeDisplayName(cu.Id, "new");

        Assert.Equal("new", db.CurrentUser.Get().DisplayedName);
    }

    [Fact]
    public void GetUserByNameWithServer_ShouldFetchAndCache()
    {
        var bl = CreateService(out var db, out _, out _);

        var user = bl.GetUserByNameWithServer("bob");

        var cached = db.Users.FindByUniqueName("bob");

        Assert.NotNull(cached);
        Assert.Equal(user.Id, cached.Id);
    }

    [Fact]
    public void UpdateContactName_ShouldUpdateLocal()
    {
        var bl = CreateService(out var db, out _, out _);

        var user = db.Users.Save(new User
        {
            Id = Guid.NewGuid(),
            UniqueName = "bob"
        });

        bl.UpdateContactName(user.Id, "Bobby");

        Assert.Equal("Bobby", db.Users.Find(user.Id).ContactName);
    }

    // ================= CHATS =================

    [Fact]
    public void CreatePrivateChat_ShouldSaveChat()
    {
        var bl = CreateService(out var db, out _, out _);

        var userId = Guid.NewGuid();
        db.Users.Save(new User { Id = userId });

        var chat = bl.CreatePrivateChat(userId, new List<Guid> { userId });

        Assert.NotNull(db.Chats.Find(chat.Id));
    }

    [Fact]
    public void GetAllChats_ShouldReturnChats()
    {
        var bl = CreateService(out var db, out _, out _);

        db.Chats.Save(new Chat { Id = Guid.NewGuid() });

        var chats = bl.GetAllChats();

        Assert.Single(chats);
    }

    [Fact]
    public void AddUserToChat_ShouldAddUser()
    {
        var bl = CreateService(out var db, out _, out _);

        var user = db.Users.Save(new User
        {
            Id = Guid.NewGuid(),
            UniqueName = "bob"
        });

        var chat = db.Chats.Save(new Chat { Id = Guid.NewGuid() });

        bl.AddUserToChat(chat.Id, "bob");

        var users = db.Chats.FindChatUsers(chat.Id);

        Assert.Contains(users, u => u.Id == user.Id);
    }

    // ================= MESSAGES =================

    [Fact]
    public void SendMessage_ShouldSaveMessage()
    {
        var bl = CreateService(out var db, out _, out _);

        var chat = db.Chats.Save(new Chat
        {
            Id = Guid.NewGuid(),
            Version = 1
        });

        var msg = bl.SendMessage(chat.Id, Guid.NewGuid(), "hello");

        var messages = db.Messages.FindChatMessages(chat.Id);

        Assert.Single(messages);
        Assert.Equal("hello", msg.Text);
    }

    [Fact]
    public void SendMessage_ShouldUpdateChatVersion()
    {
        var bl = CreateService(out var db, out _, out _);

        var chat = db.Chats.Save(new Chat
        {
            Id = Guid.NewGuid(),
            Version = 1
        });

        bl.SendMessage(chat.Id, Guid.NewGuid(), "hello");

        var updated = db.Chats.Find(chat.Id);

        Assert.True(updated.Version > 1);
    }

    [Fact]
    public void GetChatMessages_ShouldReturnMessages()
    {
        var bl = CreateService(out var db, out _, out _);

        var chatId = Guid.NewGuid();

        db.Messages.Save(new Message
        {
            MessageNumber = 1,
            ChatId = chatId,
            Text = "test"
        });

        var messages = bl.GetChatMessages(chatId);

        Assert.Single(messages);
    }

    // ================= REALTIME =================

    [Fact]
    public void OnMessageReceived_ShouldSaveMessage()
    {
        var bl = CreateService(out var db, out var rt, out _);

        var chatId = Guid.NewGuid();

        db.Chats.Save(new Chat
        {
            Id = chatId,
            Version = 1
        });

        rt.SendMessage(new Message
        {
            MessageNumber = 1,
            ChatId = chatId,
            Version = 2,
            Text = "hi"
        });

        var messages = db.Messages.FindChatMessages(chatId);

        Assert.Single(messages);
    }

    [Fact]
    public void OnMessageReceived_ShouldIgnoreDuplicate()
    {
        var bl = CreateService(out var db, out var rt, out _);

        var chatId = Guid.NewGuid();

        db.Chats.Save(new Chat { Id = chatId });

        db.Messages.Save(new Message
        {
            MessageNumber = 1,
            ChatId = chatId
        });

        rt.SendMessage(new Message
        {
            MessageNumber = 1,
            ChatId = chatId
        });

        var messages = db.Messages.FindChatMessages(chatId);

        Assert.Single(messages);
    }

    [Fact]
    public void OnMessageReceived_ShouldTriggerSync_WhenOutOfOrder()
    {
        var bl = CreateService(out var db, out var rt, out var http);

        var chatId = Guid.NewGuid();

        db.Chats.Save(new Chat
        {
            Id = chatId,
            Version = 1
        });
        
        http.SendMessage(chatId, "server msg", 0);
        
        rt.SendMessage(new Message
        {
            MessageNumber = 5,
            ChatId = chatId,
            Version = 5
        });

        var messages = db.Messages.FindChatMessages(chatId);

        Assert.NotEmpty(messages);
    }

    [Fact]
    public void OnUserLeftChat_ShouldRemoveUser()
    {
        var bl = CreateService(out var db, out var rt, out _);

        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Chats.Save(new Chat { Id = chatId });
        db.Users.Save(new User { Id = userId });
        db.Chats.AddUserToChat(chatId, userId);

        rt.NotifyUserLeftChat(chatId, userId);

        var users = db.Chats.FindChatUsers(chatId);

        Assert.DoesNotContain(users, u => u.Id == userId);
    }

    [Fact]
    public void OnChatCreated_ShouldSaveChat()
    {
        var bl = CreateService(out var db, out var rt, out _);

        var chat = new Chat
        {
            Id = Guid.NewGuid(),
            Members = new List<User>()
        };

        rt.NotifyChatCreated(chat);

        Assert.NotNull(db.Chats.Find(chat.Id));
    }
}