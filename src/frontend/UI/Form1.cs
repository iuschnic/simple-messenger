namespace UI;

public partial class Form1 : Form
{
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
        var currentUser = await _session.Messenger.GetCurrentUser();
        if (currentUser == null)
            return;

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
        var user = await _session.Messenger.Login(uniqueName, password);
        statusLabel.Text = $"Вход выполнен: {user.UniqueName}";

        Hide();

        using var messengerForm = new MessengerForm(_session);
        messengerForm.ShowDialog(this);

        Close();
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
