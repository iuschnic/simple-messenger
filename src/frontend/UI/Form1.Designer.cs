#nullable disable

namespace UI;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    private TableLayoutPanel rootLayout;
    private Panel cardPanel;
    private TableLayoutPanel cardLayout;
    private Label titleLabel;
    private Label subtitleLabel;
    private TabControl authTabs;
    private TabPage loginTab;
    private TabPage registerTab;
    private TableLayoutPanel loginLayout;
    private TableLayoutPanel registerLayout;
    private TextBox loginUniqueNameTextBox;
    private TextBox loginPasswordTextBox;
    private Button loginButton;
    private TextBox registerUniqueNameTextBox;
    private TextBox registerPasswordTextBox;
    private TextBox registerEmailTextBox;
    private TextBox registerDisplayNameTextBox;
    private Button registerButton;
    private Label statusLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        rootLayout = new TableLayoutPanel();
        cardPanel = new Panel();
        cardLayout = new TableLayoutPanel();
        titleLabel = new Label();
        subtitleLabel = new Label();
        authTabs = new TabControl();
        loginTab = new TabPage();
        registerTab = new TabPage();
        loginLayout = new TableLayoutPanel();
        loginUniqueNameTextBox = new TextBox();
        loginPasswordTextBox = new TextBox();
        loginButton = new Button();
        registerLayout = new TableLayoutPanel();
        registerUniqueNameTextBox = new TextBox();
        registerPasswordTextBox = new TextBox();
        registerEmailTextBox = new TextBox();
        registerDisplayNameTextBox = new TextBox();
        registerButton = new Button();
        statusLabel = new Label();
        rootLayout.SuspendLayout();
        cardPanel.SuspendLayout();
        cardLayout.SuspendLayout();
        authTabs.SuspendLayout();
        loginTab.SuspendLayout();
        registerTab.SuspendLayout();
        loginLayout.SuspendLayout();
        registerLayout.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayout
        // 
        rootLayout.BackColor = Color.FromArgb(236, 239, 244);
        rootLayout.ColumnCount = 3;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 460F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        rootLayout.Controls.Add(cardPanel, 1, 1);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.RowCount = 3;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 560F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        rootLayout.Location = new Point(0, 0);
        rootLayout.Name = "rootLayout";
        rootLayout.Size = new Size(1000, 700);
        rootLayout.TabIndex = 0;
        // 
        // cardPanel
        // 
        cardPanel.BackColor = Color.White;
        cardPanel.BorderStyle = BorderStyle.FixedSingle;
        cardPanel.Controls.Add(cardLayout);
        cardPanel.Dock = DockStyle.Fill;
        cardPanel.Margin = new Padding(0);
        cardPanel.Name = "cardPanel";
        cardPanel.Padding = new Padding(28);
        cardPanel.Size = new Size(460, 560);
        cardPanel.TabIndex = 0;
        // 
        // cardLayout
        // 
        cardLayout.ColumnCount = 1;
        cardLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        cardLayout.Controls.Add(titleLabel, 0, 0);
        cardLayout.Controls.Add(subtitleLabel, 0, 1);
        cardLayout.Controls.Add(authTabs, 0, 2);
        cardLayout.Controls.Add(statusLabel, 0, 3);
        cardLayout.Dock = DockStyle.Fill;
        cardLayout.RowCount = 4;
        cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        cardLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        cardLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        cardLayout.Size = new Size(402, 502);
        cardLayout.TabIndex = 0;
        // 
        // titleLabel
        // 
        titleLabel.Dock = DockStyle.Fill;
        titleLabel.Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold);
        titleLabel.Location = new Point(3, 0);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(396, 52);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "Simple Messenger";
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // subtitleLabel
        // 
        subtitleLabel.Dock = DockStyle.Fill;
        subtitleLabel.ForeColor = Color.FromArgb(90, 98, 108);
        subtitleLabel.Location = new Point(3, 52);
        subtitleLabel.Name = "subtitleLabel";
        subtitleLabel.Size = new Size(396, 48);
        subtitleLabel.TabIndex = 1;
        subtitleLabel.Text = "Сначала зарегистрируйтесь, затем войдите в приложение.";
        subtitleLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // authTabs
        // 
        authTabs.Controls.Add(loginTab);
        authTabs.Controls.Add(registerTab);
        authTabs.Dock = DockStyle.Fill;
        authTabs.Location = new Point(3, 103);
        authTabs.Name = "authTabs";
        authTabs.SelectedIndex = 0;
        authTabs.Size = new Size(396, 352);
        authTabs.TabIndex = 2;
        // 
        // loginTab
        // 
        loginTab.Controls.Add(loginLayout);
        loginTab.Location = new Point(4, 29);
        loginTab.Name = "loginTab";
        loginTab.Padding = new Padding(12);
        loginTab.Size = new Size(388, 319);
        loginTab.TabIndex = 0;
        loginTab.Text = "Вход";
        loginTab.UseVisualStyleBackColor = true;
        // 
        // registerTab
        // 
        registerTab.Controls.Add(registerLayout);
        registerTab.Location = new Point(4, 29);
        registerTab.Name = "registerTab";
        registerTab.Padding = new Padding(12);
        registerTab.Size = new Size(388, 319);
        registerTab.TabIndex = 1;
        registerTab.Text = "Регистрация";
        registerTab.UseVisualStyleBackColor = true;
        // 
        // loginLayout
        // 
        loginLayout.ColumnCount = 1;
        loginLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        loginLayout.Controls.Add(loginUniqueNameTextBox, 0, 0);
        loginLayout.Controls.Add(loginPasswordTextBox, 0, 1);
        loginLayout.Controls.Add(loginButton, 0, 2);
        loginLayout.Dock = DockStyle.Fill;
        loginLayout.RowCount = 4;
        loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        loginLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        loginLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        loginLayout.Size = new Size(364, 295);
        loginLayout.TabIndex = 0;
        // 
        // loginUniqueNameTextBox
        // 
        loginUniqueNameTextBox.Dock = DockStyle.Fill;
        loginUniqueNameTextBox.Location = new Point(6, 6);
        loginUniqueNameTextBox.Margin = new Padding(6);
        loginUniqueNameTextBox.Name = "loginUniqueNameTextBox";
        loginUniqueNameTextBox.PlaceholderText = "Unique name";
        loginUniqueNameTextBox.Size = new Size(352, 27);
        loginUniqueNameTextBox.TabIndex = 0;
        loginUniqueNameTextBox.Text = "alice";
        // 
        // loginPasswordTextBox
        // 
        loginPasswordTextBox.Dock = DockStyle.Fill;
        loginPasswordTextBox.Location = new Point(6, 56);
        loginPasswordTextBox.Margin = new Padding(6);
        loginPasswordTextBox.Name = "loginPasswordTextBox";
        loginPasswordTextBox.PlaceholderText = "Пароль";
        loginPasswordTextBox.Size = new Size(352, 27);
        loginPasswordTextBox.TabIndex = 1;
        loginPasswordTextBox.Text = "123";
        loginPasswordTextBox.UseSystemPasswordChar = true;
        // 
        // loginButton
        // 
        loginButton.BackColor = Color.FromArgb(39, 84, 138);
        loginButton.Dock = DockStyle.Fill;
        loginButton.FlatStyle = FlatStyle.Flat;
        loginButton.ForeColor = Color.White;
        loginButton.Location = new Point(6, 106);
        loginButton.Margin = new Padding(6);
        loginButton.Name = "loginButton";
        loginButton.Size = new Size(352, 40);
        loginButton.TabIndex = 2;
        loginButton.Text = "Войти";
        loginButton.UseVisualStyleBackColor = false;
        // 
        // registerLayout
        // 
        registerLayout.ColumnCount = 1;
        registerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        registerLayout.Controls.Add(registerUniqueNameTextBox, 0, 0);
        registerLayout.Controls.Add(registerPasswordTextBox, 0, 1);
        registerLayout.Controls.Add(registerEmailTextBox, 0, 2);
        registerLayout.Controls.Add(registerDisplayNameTextBox, 0, 3);
        registerLayout.Controls.Add(registerButton, 0, 4);
        registerLayout.Dock = DockStyle.Fill;
        registerLayout.RowCount = 6;
        registerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        registerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        registerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        registerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        registerLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));
        registerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        registerLayout.Size = new Size(364, 295);
        registerLayout.TabIndex = 0;
        // 
        // registerUniqueNameTextBox
        // 
        registerUniqueNameTextBox.Dock = DockStyle.Fill;
        registerUniqueNameTextBox.Location = new Point(6, 6);
        registerUniqueNameTextBox.Margin = new Padding(6);
        registerUniqueNameTextBox.Name = "registerUniqueNameTextBox";
        registerUniqueNameTextBox.PlaceholderText = "Unique name";
        registerUniqueNameTextBox.Size = new Size(352, 27);
        registerUniqueNameTextBox.TabIndex = 0;
        registerUniqueNameTextBox.Text = "alice";
        // 
        // registerPasswordTextBox
        // 
        registerPasswordTextBox.Dock = DockStyle.Fill;
        registerPasswordTextBox.Location = new Point(6, 56);
        registerPasswordTextBox.Margin = new Padding(6);
        registerPasswordTextBox.Name = "registerPasswordTextBox";
        registerPasswordTextBox.PlaceholderText = "Пароль";
        registerPasswordTextBox.Size = new Size(352, 27);
        registerPasswordTextBox.TabIndex = 1;
        registerPasswordTextBox.Text = "123";
        registerPasswordTextBox.UseSystemPasswordChar = true;
        // 
        // registerEmailTextBox
        // 
        registerEmailTextBox.Dock = DockStyle.Fill;
        registerEmailTextBox.Location = new Point(6, 106);
        registerEmailTextBox.Margin = new Padding(6);
        registerEmailTextBox.Name = "registerEmailTextBox";
        registerEmailTextBox.PlaceholderText = "Email";
        registerEmailTextBox.Size = new Size(352, 27);
        registerEmailTextBox.TabIndex = 2;
        registerEmailTextBox.Text = "alice@mail.com";
        // 
        // registerDisplayNameTextBox
        // 
        registerDisplayNameTextBox.Dock = DockStyle.Fill;
        registerDisplayNameTextBox.Location = new Point(6, 156);
        registerDisplayNameTextBox.Margin = new Padding(6);
        registerDisplayNameTextBox.Name = "registerDisplayNameTextBox";
        registerDisplayNameTextBox.PlaceholderText = "Отображаемое имя";
        registerDisplayNameTextBox.Size = new Size(352, 27);
        registerDisplayNameTextBox.TabIndex = 3;
        registerDisplayNameTextBox.Text = "Alice";
        // 
        // registerButton
        // 
        registerButton.BackColor = Color.FromArgb(38, 120, 87);
        registerButton.Dock = DockStyle.Fill;
        registerButton.FlatStyle = FlatStyle.Flat;
        registerButton.ForeColor = Color.White;
        registerButton.Location = new Point(6, 206);
        registerButton.Margin = new Padding(6);
        registerButton.Name = "registerButton";
        registerButton.Size = new Size(352, 40);
        registerButton.TabIndex = 4;
        registerButton.Text = "Создать аккаунт";
        registerButton.UseVisualStyleBackColor = false;
        // 
        // statusLabel
        // 
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.ForeColor = Color.FromArgb(90, 98, 108);
        statusLabel.Location = new Point(3, 458);
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(396, 44);
        statusLabel.TabIndex = 3;
        statusLabel.Text = "Ожидание действий";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 700);
        Controls.Add(rootLayout);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Авторизация";
        rootLayout.ResumeLayout(false);
        cardPanel.ResumeLayout(false);
        cardLayout.ResumeLayout(false);
        authTabs.ResumeLayout(false);
        loginTab.ResumeLayout(false);
        registerTab.ResumeLayout(false);
        loginLayout.ResumeLayout(false);
        loginLayout.PerformLayout();
        registerLayout.ResumeLayout(false);
        registerLayout.PerformLayout();
        ResumeLayout(false);
    }
}
