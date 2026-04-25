using BL.Interfaces;
using BL.Models;

namespace UI;

public partial class Form1 : Form
{
    private readonly IMessengerService _messenger;
    private readonly FakeRealtimeClient _realtimeClient;

    private CurrentUser? _currentUser;
    private Chat? _selectedChat;

    internal Form1(IMessengerService messenger, FakeRealtimeClient realtimeClient)
    {
        _messenger = messenger;
        _realtimeClient = realtimeClient;

        InitializeComponent();
        WireEvents();
    }

    private void WireEvents()
    {
        registerButton.Click += async (_, _) => await RunAction(RegisterAsync);
        loginButton.Click += async (_, _) => await RunAction(LoginAsync);
        currentUserButton.Click += async (_, _) => await RunAction(LoadCurrentUserAsync);
        updateDisplayNameButton.Click += async (_, _) => await RunAction(UpdateDisplayNameAsync);
        searchUserButton.Click += async (_, _) => await RunAction(SearchUserAsync);
        loadContactsButton.Click += async (_, _) => await RunAction(LoadContactsAsync);
        updateContactButton.Click += async (_, _) => await RunAction(UpdateContactAsync);
        createPrivateChatButton.Click += async (_, _) => await RunAction(CreatePrivateChatAsync);
        createGroupChatButton.Click += async (_, _) => await RunAction(CreateGroupChatAsync);
        refreshChatsButton.Click += async (_, _) => await RunAction(() => RefreshChatsAsync());
        participantsButton.Click += async (_, _) => await RunAction(LoadParticipantsAsync);
        markReadButton.Click += async (_, _) => await RunAction(MarkAsReadAsync);
        leaveChatButton.Click += async (_, _) => await RunAction(LeaveChatAsync);
        sendMessageButton.Click += async (_, _) => await RunAction(SendMessageAsync);
        simulateReconnectButton.Click += async (_, _) => await RunAction(() => _realtimeClient.EmitReconnected());
        chatsListBox.SelectedIndexChanged += async (_, _) => await RunAction(OnChatSelectedAsync);

        _messenger.Events.MessageReceived += message =>
        {
            PostToUi(async () =>
            {
                AddLog($"[EVENT] Новое сообщение [{message.MessageNumber}] {message.Text}");

                if (_selectedChat?.Id == message.ChatId)
                    await RefreshMessagesAsync();

                await RefreshChatsAsync();
            });

            return Task.CompletedTask;
        };

        _messenger.Events.ChatCreated += chat =>
        {
            PostToUi(async () =>
            {
                AddLog($"[EVENT] Создан чат {chat.Id}");
                await RefreshChatsAsync(chat.Id);
            });

            return Task.CompletedTask;
        };

        _messenger.Events.UserLeftChat += (chatId, userId) =>
        {
            PostToUi(async () =>
            {
                AddLog($"[EVENT] Пользователь {userId} вышел из чата {chatId}");
                await RefreshChatsAsync();
                await LoadParticipantsAsync();
            });

            return Task.CompletedTask;
        };

        _messenger.Events.ReconnectedToHub += () =>
        {
            PostToUi(async () =>
            {
                AddLog("[EVENT] Переподключение к хабу");
                await RefreshChatsAsync();
                await RefreshMessagesAsync();
            });

            return Task.CompletedTask;
        };

        Load += async (_, _) => await RunAction(InitializeScreenAsync);
    }

    private async Task InitializeScreenAsync()
    {
        AddLog("UI инициализирован");
        await LoadCurrentUserAsync();
        await RefreshChatsAsync();
    }

    private async Task RegisterAsync()
    {
        var user = await _messenger.RegisterUser(
            uniqueNameTextBox.Text.Trim(),
            passwordTextBox.Text,
            emailTextBox.Text.Trim(),
            displayNameTextBox.Text.Trim());

        AddLog($"REGISTERED: {user.Id} {user.UniqueName}");
        await LoadCurrentUserAsync();
    }

    private async Task LoginAsync()
    {
        var user = await _messenger.Login(uniqueNameTextBox.Text.Trim(), passwordTextBox.Text);
        AddLog($"LOGGED IN: {user.Id} {user.UniqueName}");
        await LoadCurrentUserAsync();
        await RefreshChatsAsync();
    }

    private async Task LoadCurrentUserAsync()
    {
        _currentUser = await _messenger.GetCurrentUser();

        currentUserLabel.Text = _currentUser == null
            ? "Не авторизован"
            : $"Текущий: {_currentUser.UniqueName} | display: {_currentUser.DisplayedName} | id: {_currentUser.Id}";
    }

    private async Task UpdateDisplayNameAsync()
    {
        EnsureCurrentUser();

        var updated = await _messenger.UpdateMeDisplayName(_currentUser!.Id, displayNameTextBox.Text.Trim());
        AddLog($"UPDATED USER: {updated.Id} {updated.UniqueName}");
        await LoadCurrentUserAsync();
    }

    private async Task SearchUserAsync()
    {
        var uniqueName = searchUserTextBox.Text.Trim();
        var user = await _messenger.GetUserByNameWithServer(uniqueName);

        usersListBox.DataSource = new List<User> { user };
        usersListBox.DisplayMember = nameof(User.UniqueName);
        usersListBox.SelectedIndex = 0;
        contactNameTextBox.Text = user.ContactName ?? string.Empty;

        AddLog($"USER: {user.Id} {user.UniqueName}");
    }

    private async Task LoadContactsAsync()
    {
        var contacts = await _messenger.FindUsersWithContactName();
        usersListBox.DataSource = contacts;
        usersListBox.DisplayMember = nameof(User.UniqueName);

        if (contacts.Count > 0)
            usersListBox.SelectedIndex = 0;

        AddLog($"CONTACTS COUNT: {contacts.Count}");
    }

    private async Task UpdateContactAsync()
    {
        var user = GetSelectedUser();
        var contactName = contactNameTextBox.Text.Trim();

        var updated = await _messenger.UpdateContactName(user.Id, contactName);
        AddLog($"CONTACT UPDATED: {updated.Id} {updated.ContactName}");
        await SearchUserAsync();
    }

    private async Task CreatePrivateChatAsync()
    {
        EnsureCurrentUser();
        var user = GetSelectedUser();

        var chat = await _messenger.CreatePrivateChat(_currentUser!.Id, new List<Guid> { _currentUser.Id, user.Id });
        AddLog($"PRIVATE CHAT: {chat.Id}");
        await RefreshChatsAsync(chat.Id);
    }

    private async Task CreateGroupChatAsync()
    {
        EnsureCurrentUser();

        var members = new List<Guid> { _currentUser!.Id };
        var uniqueNames = groupMembersTextBox.Text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var uniqueName in uniqueNames)
        {
            var user = await _messenger.GetUserByNameWithServer(uniqueName);
            members.Add(user.Id);
        }

        var chat = await _messenger.CreateGroupChat(groupNameTextBox.Text.Trim(), _currentUser.Id, members.Distinct().ToList());
        AddLog($"GROUP CHAT: {chat.Id}");
        await RefreshChatsAsync(chat.Id);
    }

    private async Task RefreshChatsAsync(Guid? selectChatId = null)
    {
        var chats = await _messenger.GetAllChats();

        chatsListBox.DataSource = null;
        chatsListBox.DataSource = chats;
        chatsListBox.DisplayMember = nameof(Chat.Name);

        AddLog($"CHATS COUNT: {chats.Count}");

        var chatToSelect = chats.FirstOrDefault(c => c.Id == (selectChatId ?? _selectedChat?.Id));
        if (chatToSelect != null)
        {
            chatsListBox.SelectedItem = chatToSelect;
        }
        else if (chats.Count > 0)
        {
            chatsListBox.SelectedIndex = 0;
        }
        else
        {
            _selectedChat = null;
            messagesListBox.DataSource = null;
            participantsListBox.DataSource = null;
        }
    }

    private async Task OnChatSelectedAsync()
    {
        _selectedChat = chatsListBox.SelectedItem as Chat;
        await RefreshMessagesAsync();
        await LoadParticipantsAsync();
    }

    private async Task RefreshMessagesAsync()
    {
        if (_selectedChat == null)
        {
            messagesListBox.DataSource = null;
            return;
        }

        var messages = await _messenger.GetChatMessages(_selectedChat.Id);
        messagesListBox.DataSource = null;
        messagesListBox.DataSource = messages
            .Select(m => $"[{m.MessageNumber}] {m.SenderId}: {m.Text}")
            .ToList();
    }

    private async Task LoadParticipantsAsync()
    {
        if (_selectedChat == null)
        {
            participantsListBox.DataSource = null;
            return;
        }

        var participants = await _messenger.GetChatParticipants(_selectedChat.Id);
        participantsListBox.DataSource = null;
        participantsListBox.DataSource = participants
            .Select(u => $"{u.UniqueName} ({u.ContactName ?? u.DisplayName ?? "-"})")
            .ToList();

        AddLog($"PARTICIPANTS COUNT: {participants.Count}");
    }

    private async Task SendMessageAsync()
    {
        EnsureCurrentUser();
        EnsureSelectedChat();

        var text = messageTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Введите текст сообщения");

        var message = await _messenger.SendMessage(_selectedChat!.Id, _currentUser!.Id, text);
        AddLog($"MESSAGE SENT: {message.MessageNumber} | {message.Text}");

        messageTextBox.Clear();
        await RefreshMessagesAsync();
        await RefreshChatsAsync(_selectedChat.Id);
    }

    private async Task MarkAsReadAsync()
    {
        EnsureCurrentUser();
        EnsureSelectedChat();

        await _messenger.UpdateLastReadMessageNum(_selectedChat!.Id, _currentUser!.Id);
        AddLog($"CHAT READ: {_selectedChat.Id}");
    }

    private async Task LeaveChatAsync()
    {
        EnsureCurrentUser();
        EnsureSelectedChat();

        await _messenger.LeaveChat(_selectedChat!.Id, _currentUser!.Id);
        AddLog($"LEFT CHAT: {_selectedChat.Id}");
        _selectedChat = null;
        await RefreshChatsAsync();
    }

    private User GetSelectedUser()
        => usersListBox.SelectedItem as User
           ?? throw new InvalidOperationException("Выберите пользователя");

    private void EnsureCurrentUser()
    {
        if (_currentUser == null)
            throw new InvalidOperationException("Сначала выполните регистрацию или логин");
    }

    private void EnsureSelectedChat()
    {
        if (_selectedChat == null)
            throw new InvalidOperationException("Выберите чат");
    }

    private async Task RunAction(Func<Task> action)
    {
        ToggleBusy(false);

        try
        {
            await action();
        }
        catch (Exception ex)
        {
            AddLog($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleBusy(true);
        }
    }

    private void ToggleBusy(bool enabled)
    {
        registerButton.Enabled = enabled;
        loginButton.Enabled = enabled;
        currentUserButton.Enabled = enabled;
        updateDisplayNameButton.Enabled = enabled;
        searchUserButton.Enabled = enabled;
        loadContactsButton.Enabled = enabled;
        updateContactButton.Enabled = enabled;
        createPrivateChatButton.Enabled = enabled;
        createGroupChatButton.Enabled = enabled;
        refreshChatsButton.Enabled = enabled;
        participantsButton.Enabled = enabled;
        markReadButton.Enabled = enabled;
        leaveChatButton.Enabled = enabled;
        sendMessageButton.Enabled = enabled;
        simulateReconnectButton.Enabled = enabled;
    }

    private void AddLog(string text)
    {
        logListBox.Items.Insert(0, $"{DateTime.Now:HH:mm:ss} {text}");
    }

    private void PostToUi(Func<Task> action)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        BeginInvoke(async () =>
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                AddLog($"ERROR: {ex.Message}");
            }
        });
    }
}
