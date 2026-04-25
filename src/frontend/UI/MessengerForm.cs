using BL.Models;

namespace UI;

internal partial class MessengerForm : Form
{
    private readonly UiSession _session;

    private CurrentUser? _currentUser;
    private User? _me;
    private Chat? _selectedChat;

    internal MessengerForm(UiSession session)
    {
        _session = session;
        InitializeComponent();
        WireEvents();
    }

    private void WireEvents()
    {
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
        simulateReconnectButton.Click += async (_, _) => await RunAction(() => _session.RealtimeClient.EmitReconnected());
        chatsListBox.SelectedIndexChanged += async (_, _) => await RunAction(OnChatSelectedAsync);

        _session.Messenger.Events.MessageReceived += message =>
        {
            PostToUi(async () =>
            {
                AddLog($"[EVENT] Новое сообщение [{message.MessageNumber}] {message.Text}");

                if (_selectedChat?.Id == message.ChatId)
                    await RefreshMessagesAsync();

                await RefreshChatsAsync(_selectedChat?.Id);
            });

            return Task.CompletedTask;
        };

        _session.Messenger.Events.ChatCreated += chat =>
        {
            PostToUi(async () =>
            {
                AddLog($"[EVENT] Создан чат {chat.Name} ({chat.Id})");
                await RefreshChatsAsync(chat.Id);
            });

            return Task.CompletedTask;
        };

        _session.Messenger.Events.UserLeftChat += (chatId, userId) =>
        {
            PostToUi(async () =>
            {
                AddLog($"[EVENT] Пользователь {userId} вышел из чата");
                await RefreshChatsAsync(_selectedChat?.Id);

                if (_selectedChat?.Id == chatId)
                    await LoadParticipantsAsync();
            });

            return Task.CompletedTask;
        };

        _session.Messenger.Events.ReconnectedToHub += () =>
        {
            PostToUi(async () =>
            {
                AddLog("[EVENT] Переподключение к хабу");
                await RefreshChatsAsync(_selectedChat?.Id);
                await RefreshMessagesAsync();
            });

            return Task.CompletedTask;
        };

        Load += async (_, _) => await RunAction(InitializeScreenAsync);
    }

    private async Task InitializeScreenAsync()
    {
        _currentUser = await _session.Messenger.GetCurrentUser();
        _me = _session.HttpClient.GetLoggedInUser();

        currentUserLabel.Text = _currentUser == null || _me == null
            ? "Не авторизован"
            : $"{_currentUser.DisplayedName} (@{_me.UniqueName})";

        AddLog("Окно мессенджера открыто");
        await RefreshChatsAsync();
    }

    private async Task SearchUserAsync()
    {
        var uniqueName = searchUserTextBox.Text.Trim();
        var user = await _session.Messenger.GetUserByNameWithServer(uniqueName);

        usersListBox.DataSource = new List<User> { user };
        usersListBox.DisplayMember = nameof(User.UniqueName);
        usersListBox.SelectedIndex = 0;
        contactNameTextBox.Text = user.ContactName ?? string.Empty;

        AddLog($"Найден пользователь: {user.UniqueName}");
    }

    private async Task LoadContactsAsync()
    {
        var contacts = await _session.Messenger.FindUsersWithContactName();
        usersListBox.DataSource = contacts;
        usersListBox.DisplayMember = nameof(User.UniqueName);

        if (contacts.Count > 0)
            usersListBox.SelectedIndex = 0;

        AddLog($"Контактов: {contacts.Count}");
    }

    private async Task UpdateContactAsync()
    {
        var user = GetSelectedUser();
        var updated = await _session.Messenger.UpdateContactName(user.Id, contactNameTextBox.Text.Trim());
        AddLog($"Контакт обновлён: {updated.UniqueName}");
        await SearchUserAsync();
    }

    private async Task CreatePrivateChatAsync()
    {
        EnsureLoggedInUser();

        var user = GetSelectedUser();
        var chat = await _session.Messenger.CreatePrivateChat(_me!.Id, new List<Guid> { _me.Id, user.Id });
        AddLog($"Создан личный чат: {chat.Name}");
        await RefreshChatsAsync(chat.Id);
    }

    private async Task CreateGroupChatAsync()
    {
        EnsureLoggedInUser();

        var members = new List<Guid> { _me!.Id };
        var uniqueNames = groupMembersTextBox.Text
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var uniqueName in uniqueNames)
        {
            var user = await _session.Messenger.GetUserByNameWithServer(uniqueName);
            members.Add(user.Id);
        }

        var chat = await _session.Messenger.CreateGroupChat(
            groupNameTextBox.Text.Trim(),
            _me.Id,
            members.Distinct().ToList());

        AddLog($"Создан групповой чат: {chat.Name}");
        await RefreshChatsAsync(chat.Id);
    }

    private async Task RefreshChatsAsync(Guid? selectChatId = null)
    {
        var chats = await _session.Messenger.GetAllChats();
        var items = chats.Select(c => new ChatListItem(c)).ToList();

        chatsListBox.DataSource = null;
        chatsListBox.DataSource = items;

        var targetId = selectChatId ?? _selectedChat?.Id;
        var target = items.FirstOrDefault(i => i.Chat.Id == targetId);

        if (target != null)
        {
            chatsListBox.SelectedItem = target;
        }
        else if (items.Count > 0)
        {
            chatsListBox.SelectedIndex = 0;
        }
        else
        {
            _selectedChat = null;
            activeChatLabel.Text = "Выберите чат";
            messagesListBox.DataSource = null;
            participantsListBox.DataSource = null;
        }
    }

    private async Task OnChatSelectedAsync()
    {
        _selectedChat = (chatsListBox.SelectedItem as ChatListItem)?.Chat;
        activeChatLabel.Text = _selectedChat == null ? "Выберите чат" : _selectedChat.Name;
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

        var messages = await _session.Messenger.GetChatMessages(_selectedChat.Id);
        var items = messages.Select(m =>
        {
            var sender = m.SenderId == _me?.Id ? "Вы" : ShortGuid(m.SenderId);
            return $"[{m.MessageNumber}] {sender}: {m.Text}";
        }).ToList();

        messagesListBox.DataSource = null;
        messagesListBox.DataSource = items;
    }

    private async Task LoadParticipantsAsync()
    {
        if (_selectedChat == null)
        {
            participantsListBox.DataSource = null;
            return;
        }

        var participants = await _session.Messenger.GetChatParticipants(_selectedChat.Id);
        participantsListBox.DataSource = null;
        participantsListBox.DataSource = participants
            .Select(u => $"{u.UniqueName} ({u.ContactName ?? u.DisplayName})")
            .ToList();
    }

    private async Task SendMessageAsync()
    {
        EnsureLoggedInUser();
        EnsureSelectedChat();

        var text = messageTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("Введите текст сообщения");

        var message = await _session.Messenger.SendMessage(_selectedChat!.Id, _me!.Id, text);
        AddLog($"Сообщение отправлено: {message.MessageNumber}");
        messageTextBox.Clear();

        await RefreshMessagesAsync();
        await RefreshChatsAsync(_selectedChat.Id);
    }

    private async Task MarkAsReadAsync()
    {
        EnsureLoggedInUser();
        EnsureSelectedChat();

        await _session.Messenger.UpdateLastReadMessageNum(_selectedChat!.Id, _me!.Id);
        AddLog("Чат отмечен прочитанным");
    }

    private async Task LeaveChatAsync()
    {
        EnsureLoggedInUser();
        EnsureSelectedChat();

        await _session.Messenger.LeaveChat(_selectedChat!.Id, _me!.Id);
        AddLog("Вы вышли из чата");
        _selectedChat = null;
        await RefreshChatsAsync();
    }

    private User GetSelectedUser()
        => usersListBox.SelectedItem as User
           ?? throw new InvalidOperationException("Сначала выберите пользователя");

    private void EnsureLoggedInUser()
    {
        if (_me == null)
            throw new InvalidOperationException("Сначала войдите в приложение");
    }

    private void EnsureSelectedChat()
    {
        if (_selectedChat == null)
            throw new InvalidOperationException("Сначала выберите чат");
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
            AddLog($"Ошибка: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            ToggleBusy(true);
        }
    }

    private void ToggleBusy(bool enabled)
    {
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
                AddLog($"Ошибка: {ex.Message}");
            }
        });
    }

    private static string ShortGuid(Guid id)
        => id.ToString()[..8];

    private sealed class ChatListItem
    {
        public ChatListItem(Chat chat)
        {
            Chat = chat;
        }

        public Chat Chat { get; }

        public override string ToString()
            => string.IsNullOrWhiteSpace(Chat.Name)
                ? $"Chat {Chat.Id.ToString()[..8]}"
                : Chat.Name;
    }
}
