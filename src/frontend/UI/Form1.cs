using BL.Exceptions;
using BL.Models;

namespace UI;

public partial class Form1 : Form
{
    private const int ServerSyncRetryCount = 5;
    private static readonly TimeSpan ServerSyncRetryDelay = TimeSpan.FromMilliseconds(700);

    private readonly UiSession _session;

    internal Form1(UiSession session)
    {
        _session = session;
        InitializeComponent();
        WireEvents();
    }

    private void WireEvents()
    {
        registerButton.Click += async (_, _) => await RunAction(RegisterAsync);
        loginButton.Click += async (_, _) => await RunAction(LoginAsync);
        Load += async (_, _) => await RunAction(InitializeAsync);
    }

    private async Task InitializeAsync()
    {
        authTabs.SelectedTab = registerTab;

        var loginAgainResult = await RetryServerSyncAsync(() => _session.Messenger.LoginAgain());
        if (loginAgainResult == ReturnCode.Success)
        {
            OpenMessenger();
            return;
        }

        var currentUser = await _session.Messenger.GetCurrentUser();
        if (currentUser == null)
        {
            statusLabel.Text = "Локальный пользователь не найден. Зарегистрируйтесь, затем войдите.";
            return;
        }

        loginUniqueNameTextBox.Text = currentUser.UniqueName;
        loginPasswordTextBox.Text = currentUser.PasswordHash;
        registerUniqueNameTextBox.Text = currentUser.UniqueName;
        registerPasswordTextBox.Text = currentUser.PasswordHash;
        registerEmailTextBox.Text = currentUser.Email;
        registerDisplayNameTextBox.Text = currentUser.DisplayedName;
        statusLabel.Text = $"Найден локальный пользователь: {currentUser.UniqueName}";
    }

    private async Task RegisterAsync()
    {
        var uniqueName = registerUniqueNameTextBox.Text.Trim();
        var password = registerPasswordTextBox.Text;
        var email = registerEmailTextBox.Text.Trim();
        var displayName = registerDisplayNameTextBox.Text.Trim();

        var user = await _session.Messenger.RegisterUser(uniqueName, password, email, displayName);

        loginUniqueNameTextBox.Text = uniqueName;
        loginPasswordTextBox.Text = password;
        statusLabel.Text = $"Аккаунт создан: {user.UniqueName}. Теперь войдите на вкладке \"Вход\".";
        authTabs.SelectedTab = loginTab;
    }

    private async Task LoginAsync()
    {
        await LoginInternalAsync(loginUniqueNameTextBox.Text.Trim(), loginPasswordTextBox.Text);
    }

    private async Task LoginInternalAsync(string uniqueName, string password)
    {
        var user = await RetryServerSyncAsync(() => _session.Messenger.Login(uniqueName, password));
        statusLabel.Text = $"Вход выполнен: {user.UniqueName}";

        OpenMessenger();
    }

    private void OpenMessenger()
    {
        Hide();

        using var messengerForm = new MessengerForm(_session);
        messengerForm.ShowDialog(this);

        Close();
    }

    private async Task<T> RetryServerSyncAsync<T>(Func<Task<T>> action)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (attempt < ServerSyncRetryCount && IsServerSyncDelay(ex))
            {
                await Task.Delay(ServerSyncRetryDelay);
            }
        }
    }

    private static bool IsServerSyncDelay(Exception ex)
    {
        var message = ex.Message;

        return ex is AppException or HubConnectionException
               && (message.Contains("500", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("server", StringComparison.OrdinalIgnoreCase)
                   || message.Contains("сервер", StringComparison.OrdinalIgnoreCase));
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
            statusLabel.Text = ex.Message;
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
    }
}
