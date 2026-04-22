using BL.Exceptions;
using BL.Models;
using BL.Services;
using BL.UnitTest.Mocks;
using Xunit;

namespace BL.UnitTest;

public class MessengerServiceTests
{
    private static MessengerService CreateService(out FakeRepositoryHub db, out FakeRealtimeClient rt, out FakeHttpClient http)
    {
        http = new FakeHttpClient();
        rt = new FakeRealtimeClient();
        db = new FakeRepositoryHub();

        return new MessengerService(http, rt, db);
    }

    // ================= AUTH =================

    [Fact]
    public async Task RegisterUser_ShouldSaveCurrentUser()
    {
        var bl = CreateService(out var db, out _, out _);

        var user = await bl.RegisterUser("alice", "123", "a@mail.com", "Alice");

        var stored = await db.CurrentUser.Get();

        Assert.Equal(user.UniqueName, stored.UniqueName);
    }

    [Fact]
    public async Task Login_ShouldThrow_WhenWrongCredentials()
    {
        var bl = CreateService(out var db, out _, out _);

        await db.CurrentUser.Save(new CurrentUser
        {
            UniqueName = "alice",
            PasswordHash = "123"
        });

        var ex = await Assert.ThrowsAsync<AuthException>(() => bl.Login("alice", "wrong"));
        Assert.Equal("Неверный логин или пароль", ex.Message);
    }

    [Fact]
    public async Task Login_ShouldSaveUser()
    {
        var bl = CreateService(out var db, out _, out _);

        await db.CurrentUser.Save(new CurrentUser
        {
            UniqueName = "alice",
            PasswordHash = "123"
        });

        var user = await bl.Login("alice", "123");

        Assert.NotNull(await db.Users.Find(user.Id));
    }

    // ================= USERS =================

    [Fact]
    public async Task UpdateMeDisplayName_ShouldUpdateLocalUser()
    {
        var bl = CreateService(out var db, out _, out _);

        var cu = await db.CurrentUser.Save(new CurrentUser
        {
            UniqueName = "alice",
            DisplayedName = "old"
        });

        await bl.UpdateMeDisplayName(cu.Id, "new");

        Assert.Equal("new", (await db.CurrentUser.Get()).DisplayedName);
    }

    [Fact]
    public async Task GetUserByNameWithServer_ShouldFetchAndCache()
    {
        var bl = CreateService(out var db, out _, out _);

        var user = await bl.GetUserByNameWithServer("bob");

        var cached = await db.Users.FindByUniqueName("bob");

        Assert.NotNull(cached);
        Assert.Equal(user.Id, cached.Id);
    }

    [Fact]
    public async Task UpdateContactName_ShouldUpdateLocal()
    {
        var bl = CreateService(out var db, out _, out _);

        var user = await db.Users.Save(new User
        {
            Id = Guid.NewGuid(),
            UniqueName = "bob"
        });

        await bl.UpdateContactName(user.Id, "Bobby");

        Assert.Equal("Bobby", (await db.Users.Find(user.Id)).ContactName);
    }

    // ================= CHATS =================

    [Fact]
    public async Task CreatePrivateChat_ShouldSaveChat()
    {
        var bl = CreateService(out var db, out _, out _);

        var userId = Guid.NewGuid();
        await db.Users.Save(new User { Id = userId });

        var chat = await bl.CreatePrivateChat(userId, new List<Guid> { userId });

        Assert.NotNull(await db.Chats.Find(chat.Id));
    }

    [Fact]
    public async Task GetAllChats_ShouldReturnChats()
    {
        var bl = CreateService(out var db, out _, out _);

        await db.Chats.Save(new Chat { Id = Guid.NewGuid() });

        var chats = await bl.GetAllChats();

        Assert.Single(chats);
    }

    [Fact]
    public async Task AddUserToChat_ShouldAddUser()
    {
        var bl = CreateService(out var db, out _, out _);

        var user = await db.Users.Save(new User
        {
            Id = Guid.NewGuid(),
            UniqueName = "bob"
        });

        var chat = await db.Chats.Save(new Chat { Id = Guid.NewGuid() });

        await bl.AddUserToChat(chat.Id, "bob");

        var users = await db.Chats.FindChatUsers(chat.Id);

        Assert.Contains(users, u => u.Id == user.Id);
    }

    // ================= MESSAGES =================

    [Fact]
    public async Task SendMessage_ShouldSaveMessage()
    {
        var bl = CreateService(out var db, out _, out _);

        var chat = await db.Chats.Save(new Chat
        {
            Id = Guid.NewGuid(),
            Version = 1
        });

        var msg = await bl.SendMessage(chat.Id, Guid.NewGuid(), "hello");

        var messages = await db.Messages.FindChatMessages(chat.Id);

        Assert.Single(messages);
        Assert.Equal("hello", msg.Text);
    }

    [Fact]
    public async Task SendMessage_ShouldUpdateChatVersion()
    {
        var bl = CreateService(out var db, out _, out _);

        var chat = await db.Chats.Save(new Chat
        {
            Id = Guid.NewGuid(),
            Version = 1
        });

        await bl.SendMessage(chat.Id, Guid.NewGuid(), "hello");

        var updated = await db.Chats.Find(chat.Id);

        Assert.True(updated.Version > 1);
    }

    [Fact]
    public async Task GetChatMessages_ShouldReturnMessages()
    {
        var bl = CreateService(out var db, out _, out _);

        var chatId = Guid.NewGuid();

        await db.Messages.Save(new Message
        {
            MessageNumber = 1,
            ChatId = chatId,
            Text = "test"
        });

        var messages = await bl.GetChatMessages(chatId);

        Assert.Single(messages);
    }

    // ================= REALTIME =================

    [Fact]
    public async Task OnMessageReceived_ShouldSaveMessage_AndRaiseEvent()
    {
        var bl = CreateService(out var db, out var rt, out _);

        var chatId = Guid.NewGuid();

        await db.Chats.Save(new Chat { Id = chatId, Version = 1 });

        Message? received = null;
        bl.Events.MessageReceived += m =>
        {
            received = m;
            return Task.CompletedTask;
        };

        rt.SendMessage(new Message
        {
            MessageNumber = 1,
            ChatId = chatId,
            Version = 2,
            Text = "hi"
        });

        var messages = await db.Messages.FindChatMessages(chatId);

        Assert.Single(messages);
        Assert.NotNull(received);
        Assert.Equal("hi", received.Text);
    }

    [Fact]
    public async Task OnMessageReceived_ShouldIgnoreDuplicate_AndNotRaiseEvent()
    {
        var bl = CreateService(out var db, out var rt, out _);

        var chatId = Guid.NewGuid();

        await db.Chats.Save(new Chat { Id = chatId });

        await db.Messages.Save(new Message { MessageNumber = 1, ChatId = chatId });

        var called = false;
        bl.Events.MessageReceived += _ =>
        {
            called = true;
            return Task.CompletedTask;
        };

        rt.SendMessage(new Message { MessageNumber = 1, ChatId = chatId });

        var messages = await db.Messages.FindChatMessages(chatId);

        Assert.Single(messages);
        Assert.False(called);
    }

    [Fact]
    public async Task OnUserLeftChat_ShouldRemoveUser_AndRaiseEvent()
    {
        var bl = CreateService(out var db, out var rt, out _);

        var chatId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        await db.Chats.Save(new Chat { Id = chatId });
        await db.Users.Save(new User { Id = userId });
        await db.Chats.AddUserToChat(chatId, userId);

        bool eventCalled = false;
        bl.Events.UserLeftChat += (_, _) =>
        {
            eventCalled = true;
            return Task.CompletedTask;
        };

        rt.NotifyUserLeftChat(chatId, userId);

        var users = await db.Chats.FindChatUsers(chatId);

        Assert.DoesNotContain(users, u => u.Id == userId);
        Assert.True(eventCalled);
    }

    [Fact]
    public async Task OnChatCreated_ShouldSaveChat_AndRaiseEvent()
    {
        var bl = CreateService(out var db, out var rt, out _);

        var chat = new Chat { Id = Guid.NewGuid(), Members = new List<User>() };

        bool called = false;
        bl.Events.ChatCreated += _ =>
        {
            called = true;
            return Task.CompletedTask;
        };

        rt.NotifyChatCreated(chat);

        Assert.NotNull(await db.Chats.Find(chat.Id));
        Assert.True(called);
    }

    // ================= ERRORS =================

    [Fact]
    public async Task RegisterUser_ShouldMapApiException()
    {
        var bl = CreateService(out _, out _, out var http);

        http.ExceptionToThrow = new ApiException(400, "bad");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            bl.RegisterUser("a", "b", "c", "d"));

        Assert.Equal("bad", ex.Message);
    }

    [Fact]
    public async Task Login_ShouldMapApiException()
    {
        var bl = CreateService(out var db, out _, out var http);

        await db.CurrentUser.Save(new CurrentUser
        {
            UniqueName = "alice",
            PasswordHash = "123"
        });

        http.ExceptionToThrow = new ApiException(401, "unauthorized");

        var ex = await Assert.ThrowsAsync<AuthException>(() =>
            bl.Login("alice", "123"));

        Assert.Equal("Требуется авторизация", ex.Message);
    }

    [Fact]
    public async Task GetUserByNameWithServer_ShouldMapApiException()
    {
        var bl = CreateService(out _, out _, out var http);

        http.ExceptionToThrow = new ApiException(404, "not found");

        var ex = await Assert.ThrowsAsync<NotFoundAppException>(() =>
            bl.GetUserByNameWithServer("bob"));

        Assert.Equal("not found", ex.Message);
    }

    [Fact]
    public async Task CreateGroupChat_ShouldMapApiException()
    {
        var bl = CreateService(out _, out _, out var http);

        http.ExceptionToThrow = new ApiException(409, "conflict");

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
            bl.CreateGroupChat("test", Guid.NewGuid(), new List<Guid>())
        );

        Assert.Equal("conflict", ex.Message);
    }

    [Fact]
    public async Task SendMessage_ShouldMapApiException()
    {
        var bl = CreateService(out var db, out _, out var http);

        var chat = await db.Chats.Save(new Chat
        {
            Id = Guid.NewGuid(),
            Version = 1
        });

        http.ExceptionToThrow = new ApiException(500, "server");

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            bl.SendMessage(chat.Id, Guid.NewGuid(), "hi"));

        Assert.Equal("Ошибка сервера (500): server", ex.Message);
    }

    [Fact]
    public async Task LeaveChat_ShouldMapApiException()
    {
        var bl = CreateService(out _, out _, out var http);

        http.ExceptionToThrow = new ApiException(403, "forbidden");

        var ex = await Assert.ThrowsAsync<AuthException>(() =>
            bl.LeaveChat(Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal("Недостаточно прав", ex.Message);
    }
}

