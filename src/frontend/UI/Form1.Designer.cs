#nullable disable

namespace UI;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;

    private System.Windows.Forms.TableLayoutPanel rootLayout = null!;
    private System.Windows.Forms.GroupBox authGroup = null!;
    private System.Windows.Forms.TableLayoutPanel authLayout = null!;
    private System.Windows.Forms.TextBox uniqueNameTextBox = null!;
    private System.Windows.Forms.TextBox passwordTextBox = null!;
    private System.Windows.Forms.TextBox emailTextBox = null!;
    private System.Windows.Forms.TextBox displayNameTextBox = null!;
    private System.Windows.Forms.Button registerButton = null!;
    private System.Windows.Forms.Button loginButton = null!;
    private System.Windows.Forms.Button currentUserButton = null!;
    private System.Windows.Forms.Button updateDisplayNameButton = null!;
    private System.Windows.Forms.Label currentUserLabel = null!;

    private System.Windows.Forms.SplitContainer bodySplit = null!;
    private System.Windows.Forms.TableLayoutPanel leftLayout = null!;
    private System.Windows.Forms.GroupBox usersGroup = null!;
    private System.Windows.Forms.TableLayoutPanel usersLayout = null!;
    private System.Windows.Forms.TextBox searchUserTextBox = null!;
    private System.Windows.Forms.Button searchUserButton = null!;
    private System.Windows.Forms.Button loadContactsButton = null!;
    private System.Windows.Forms.ListBox usersListBox = null!;
    private System.Windows.Forms.TextBox contactNameTextBox = null!;
    private System.Windows.Forms.Button updateContactButton = null!;
    private System.Windows.Forms.Button createPrivateChatButton = null!;
    private System.Windows.Forms.GroupBox groupGroup = null!;
    private System.Windows.Forms.TableLayoutPanel groupLayout = null!;
    private System.Windows.Forms.TextBox groupNameTextBox = null!;
    private System.Windows.Forms.TextBox groupMembersTextBox = null!;
    private System.Windows.Forms.Button createGroupChatButton = null!;

    private System.Windows.Forms.SplitContainer centerSplit = null!;
    private System.Windows.Forms.GroupBox chatsGroup = null!;
    private System.Windows.Forms.TableLayoutPanel chatsLayout = null!;
    private System.Windows.Forms.Button refreshChatsButton = null!;
    private System.Windows.Forms.ListBox chatsListBox = null!;
    private System.Windows.Forms.Button participantsButton = null!;
    private System.Windows.Forms.Button markReadButton = null!;
    private System.Windows.Forms.Button leaveChatButton = null!;
    private System.Windows.Forms.ListBox participantsListBox = null!;

    private System.Windows.Forms.GroupBox messagesGroup = null!;
    private System.Windows.Forms.TableLayoutPanel messagesLayout = null!;
    private System.Windows.Forms.ListBox messagesListBox = null!;
    private System.Windows.Forms.TextBox messageTextBox = null!;
    private System.Windows.Forms.Button sendMessageButton = null!;

    private System.Windows.Forms.GroupBox logGroup = null!;
    private System.Windows.Forms.TableLayoutPanel logLayout = null!;
    private System.Windows.Forms.Button simulateReconnectButton = null!;
    private System.Windows.Forms.ListBox logListBox = null!;

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
        authGroup = new GroupBox();
        authLayout = new TableLayoutPanel();
        uniqueNameTextBox = new TextBox();
        passwordTextBox = new TextBox();
        emailTextBox = new TextBox();
        displayNameTextBox = new TextBox();
        registerButton = new Button();
        loginButton = new Button();
        currentUserButton = new Button();
        updateDisplayNameButton = new Button();
        currentUserLabel = new Label();
        bodySplit = new SplitContainer();
        leftLayout = new TableLayoutPanel();
        usersGroup = new GroupBox();
        usersLayout = new TableLayoutPanel();
        searchUserTextBox = new TextBox();
        searchUserButton = new Button();
        loadContactsButton = new Button();
        usersListBox = new ListBox();
        contactNameTextBox = new TextBox();
        updateContactButton = new Button();
        createPrivateChatButton = new Button();
        groupGroup = new GroupBox();
        groupLayout = new TableLayoutPanel();
        groupNameTextBox = new TextBox();
        groupMembersTextBox = new TextBox();
        createGroupChatButton = new Button();
        centerSplit = new SplitContainer();
        chatsGroup = new GroupBox();
        chatsLayout = new TableLayoutPanel();
        refreshChatsButton = new Button();
        chatsListBox = new ListBox();
        participantsButton = new Button();
        markReadButton = new Button();
        leaveChatButton = new Button();
        participantsListBox = new ListBox();
        messagesGroup = new GroupBox();
        messagesLayout = new TableLayoutPanel();
        messagesListBox = new ListBox();
        messageTextBox = new TextBox();
        sendMessageButton = new Button();
        logGroup = new GroupBox();
        logLayout = new TableLayoutPanel();
        simulateReconnectButton = new Button();
        logListBox = new ListBox();
        rootLayout.SuspendLayout();
        authGroup.SuspendLayout();
        authLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)bodySplit).BeginInit();
        bodySplit.Panel1.SuspendLayout();
        bodySplit.Panel2.SuspendLayout();
        bodySplit.SuspendLayout();
        leftLayout.SuspendLayout();
        usersGroup.SuspendLayout();
        usersLayout.SuspendLayout();
        groupGroup.SuspendLayout();
        groupLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)centerSplit).BeginInit();
        centerSplit.Panel1.SuspendLayout();
        centerSplit.Panel2.SuspendLayout();
        centerSplit.SuspendLayout();
        chatsGroup.SuspendLayout();
        chatsLayout.SuspendLayout();
        messagesGroup.SuspendLayout();
        messagesLayout.SuspendLayout();
        logGroup.SuspendLayout();
        logLayout.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayout
        // 
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(authGroup, 0, 0);
        rootLayout.Controls.Add(bodySplit, 0, 1);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.RowCount = 2;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.Location = new Point(0, 0);
        rootLayout.Name = "rootLayout";
        rootLayout.Size = new Size(1540, 900);
        rootLayout.TabIndex = 0;
        // 
        // authGroup
        // 
        authGroup.Controls.Add(authLayout);
        authGroup.Dock = DockStyle.Fill;
        authGroup.Location = new Point(8, 8);
        authGroup.Margin = new Padding(8);
        authGroup.Name = "authGroup";
        authGroup.Padding = new Padding(8);
        authGroup.Size = new Size(1524, 134);
        authGroup.TabIndex = 0;
        authGroup.TabStop = false;
        authGroup.Text = "Авторизация";
        // 
        // authLayout
        // 
        authLayout.ColumnCount = 5;
        authLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        authLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        authLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        authLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        authLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        authLayout.Controls.Add(uniqueNameTextBox, 0, 0);
        authLayout.Controls.Add(passwordTextBox, 1, 0);
        authLayout.Controls.Add(emailTextBox, 2, 0);
        authLayout.Controls.Add(displayNameTextBox, 3, 0);
        authLayout.Controls.Add(registerButton, 4, 0);
        authLayout.Controls.Add(loginButton, 4, 1);
        authLayout.Controls.Add(currentUserButton, 3, 1);
        authLayout.Controls.Add(updateDisplayNameButton, 2, 1);
        authLayout.Controls.Add(currentUserLabel, 0, 1);
        authLayout.Dock = DockStyle.Fill;
        authLayout.Location = new Point(8, 24);
        authLayout.Name = "authLayout";
        authLayout.RowCount = 2;
        authLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        authLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        authLayout.Size = new Size(1508, 102);
        authLayout.TabIndex = 0;
        // 
        // uniqueNameTextBox
        // 
        uniqueNameTextBox.Dock = DockStyle.Fill;
        uniqueNameTextBox.Location = new Point(6, 6);
        uniqueNameTextBox.Margin = new Padding(6);
        uniqueNameTextBox.Name = "uniqueNameTextBox";
        uniqueNameTextBox.PlaceholderText = "unique name";
        uniqueNameTextBox.Size = new Size(289, 27);
        uniqueNameTextBox.TabIndex = 0;
        uniqueNameTextBox.Text = "alice";
        // 
        // passwordTextBox
        // 
        passwordTextBox.Dock = DockStyle.Fill;
        passwordTextBox.Location = new Point(307, 6);
        passwordTextBox.Margin = new Padding(6);
        passwordTextBox.Name = "passwordTextBox";
        passwordTextBox.PlaceholderText = "password";
        passwordTextBox.Size = new Size(289, 27);
        passwordTextBox.TabIndex = 1;
        passwordTextBox.Text = "123";
        // 
        // emailTextBox
        // 
        emailTextBox.Dock = DockStyle.Fill;
        emailTextBox.Location = new Point(608, 6);
        emailTextBox.Margin = new Padding(6);
        emailTextBox.Name = "emailTextBox";
        emailTextBox.PlaceholderText = "email";
        emailTextBox.Size = new Size(289, 27);
        emailTextBox.TabIndex = 2;
        emailTextBox.Text = "alice@mail.com";
        // 
        // displayNameTextBox
        // 
        displayNameTextBox.Dock = DockStyle.Fill;
        displayNameTextBox.Location = new Point(909, 6);
        displayNameTextBox.Margin = new Padding(6);
        displayNameTextBox.Name = "displayNameTextBox";
        displayNameTextBox.PlaceholderText = "display name";
        displayNameTextBox.Size = new Size(289, 27);
        displayNameTextBox.TabIndex = 3;
        displayNameTextBox.Text = "Alice";
        // 
        // registerButton
        // 
        registerButton.Dock = DockStyle.Fill;
        registerButton.Location = new Point(1210, 6);
        registerButton.Margin = new Padding(6);
        registerButton.Name = "registerButton";
        registerButton.Size = new Size(292, 39);
        registerButton.TabIndex = 4;
        registerButton.Text = "Регистрация";
        registerButton.UseVisualStyleBackColor = true;
        // 
        // loginButton
        // 
        loginButton.Dock = DockStyle.Fill;
        loginButton.Location = new Point(1210, 57);
        loginButton.Margin = new Padding(6);
        loginButton.Name = "loginButton";
        loginButton.Size = new Size(292, 39);
        loginButton.TabIndex = 5;
        loginButton.Text = "Логин";
        loginButton.UseVisualStyleBackColor = true;
        // 
        // currentUserButton
        // 
        currentUserButton.Dock = DockStyle.Fill;
        currentUserButton.Location = new Point(909, 57);
        currentUserButton.Margin = new Padding(6);
        currentUserButton.Name = "currentUserButton";
        currentUserButton.Size = new Size(289, 39);
        currentUserButton.TabIndex = 6;
        currentUserButton.Text = "Текущий пользователь";
        currentUserButton.UseVisualStyleBackColor = true;
        // 
        // updateDisplayNameButton
        // 
        updateDisplayNameButton.Dock = DockStyle.Fill;
        updateDisplayNameButton.Location = new Point(608, 57);
        updateDisplayNameButton.Margin = new Padding(6);
        updateDisplayNameButton.Name = "updateDisplayNameButton";
        updateDisplayNameButton.Size = new Size(289, 39);
        updateDisplayNameButton.TabIndex = 7;
        updateDisplayNameButton.Text = "Обновить имя";
        updateDisplayNameButton.UseVisualStyleBackColor = true;
        // 
        // currentUserLabel
        // 
        authLayout.SetColumnSpan(currentUserLabel, 2);
        currentUserLabel.Dock = DockStyle.Fill;
        currentUserLabel.Location = new Point(6, 51);
        currentUserLabel.Margin = new Padding(6, 0, 6, 0);
        currentUserLabel.Name = "currentUserLabel";
        currentUserLabel.Size = new Size(590, 51);
        currentUserLabel.TabIndex = 8;
        currentUserLabel.Text = "Не авторизован";
        currentUserLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // bodySplit
        // 
        bodySplit.Dock = DockStyle.Fill;
        bodySplit.Location = new Point(8, 158);
        bodySplit.Margin = new Padding(8);
        bodySplit.Name = "bodySplit";
        // 
        // bodySplit.Panel1
        // 
        bodySplit.Panel1.Controls.Add(leftLayout);
        // 
        // bodySplit.Panel2
        // 
        bodySplit.Panel2.Controls.Add(centerSplit);
        bodySplit.Size = new Size(1524, 734);
        bodySplit.SplitterDistance = 420;
        bodySplit.TabIndex = 1;
        // 
        // leftLayout
        // 
        leftLayout.ColumnCount = 1;
        leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        leftLayout.Controls.Add(usersGroup, 0, 0);
        leftLayout.Controls.Add(groupGroup, 0, 1);
        leftLayout.Dock = DockStyle.Fill;
        leftLayout.RowCount = 2;
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 68F));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 32F));
        leftLayout.Location = new Point(0, 0);
        leftLayout.Name = "leftLayout";
        leftLayout.Size = new Size(420, 734);
        leftLayout.TabIndex = 0;
        // 
        // usersGroup
        // 
        usersGroup.Controls.Add(usersLayout);
        usersGroup.Dock = DockStyle.Fill;
        usersGroup.Location = new Point(8, 8);
        usersGroup.Margin = new Padding(8);
        usersGroup.Name = "usersGroup";
        usersGroup.Padding = new Padding(8);
        usersGroup.Size = new Size(404, 483);
        usersGroup.TabIndex = 0;
        usersGroup.TabStop = false;
        usersGroup.Text = "Пользователи и контакты";
        // 
        // usersLayout
        // 
        usersLayout.ColumnCount = 2;
        usersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        usersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        usersLayout.Controls.Add(searchUserTextBox, 0, 0);
        usersLayout.Controls.Add(searchUserButton, 1, 0);
        usersLayout.Controls.Add(loadContactsButton, 0, 1);
        usersLayout.Controls.Add(usersListBox, 0, 2);
        usersLayout.Controls.Add(contactNameTextBox, 0, 3);
        usersLayout.Controls.Add(updateContactButton, 1, 3);
        usersLayout.Controls.Add(createPrivateChatButton, 0, 4);
        usersLayout.Dock = DockStyle.Fill;
        usersLayout.Location = new Point(8, 24);
        usersLayout.Name = "usersLayout";
        usersLayout.RowCount = 5;
        usersLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        usersLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        usersLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        usersLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        usersLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        usersLayout.Size = new Size(388, 451);
        usersLayout.TabIndex = 0;
        // 
        // searchUserTextBox
        // 
        searchUserTextBox.Dock = DockStyle.Fill;
        searchUserTextBox.Location = new Point(6, 6);
        searchUserTextBox.Margin = new Padding(6);
        searchUserTextBox.Name = "searchUserTextBox";
        searchUserTextBox.PlaceholderText = "unique name";
        searchUserTextBox.Size = new Size(182, 27);
        searchUserTextBox.TabIndex = 0;
        searchUserTextBox.Text = "stass";
        // 
        // searchUserButton
        // 
        searchUserButton.Dock = DockStyle.Fill;
        searchUserButton.Location = new Point(200, 6);
        searchUserButton.Margin = new Padding(6);
        searchUserButton.Name = "searchUserButton";
        searchUserButton.Size = new Size(182, 30);
        searchUserButton.TabIndex = 1;
        searchUserButton.Text = "Найти на сервере";
        searchUserButton.UseVisualStyleBackColor = true;
        // 
        // loadContactsButton
        // 
        usersLayout.SetColumnSpan(loadContactsButton, 2);
        loadContactsButton.Dock = DockStyle.Fill;
        loadContactsButton.Location = new Point(6, 48);
        loadContactsButton.Margin = new Padding(6);
        loadContactsButton.Name = "loadContactsButton";
        loadContactsButton.Size = new Size(376, 30);
        loadContactsButton.TabIndex = 2;
        loadContactsButton.Text = "Загрузить контакты";
        loadContactsButton.UseVisualStyleBackColor = true;
        // 
        // usersListBox
        // 
        usersLayout.SetColumnSpan(usersListBox, 2);
        usersListBox.Dock = DockStyle.Fill;
        usersListBox.FormattingEnabled = true;
        usersListBox.Location = new Point(6, 90);
        usersListBox.Margin = new Padding(6);
        usersListBox.Name = "usersListBox";
        usersListBox.Size = new Size(376, 271);
        usersListBox.TabIndex = 3;
        // 
        // contactNameTextBox
        // 
        contactNameTextBox.Dock = DockStyle.Fill;
        contactNameTextBox.Location = new Point(6, 373);
        contactNameTextBox.Margin = new Padding(6);
        contactNameTextBox.Name = "contactNameTextBox";
        contactNameTextBox.PlaceholderText = "новое имя контакта";
        contactNameTextBox.Size = new Size(182, 27);
        contactNameTextBox.TabIndex = 4;
        // 
        // updateContactButton
        // 
        updateContactButton.Dock = DockStyle.Fill;
        updateContactButton.Location = new Point(200, 373);
        updateContactButton.Margin = new Padding(6);
        updateContactButton.Name = "updateContactButton";
        updateContactButton.Size = new Size(182, 30);
        updateContactButton.TabIndex = 5;
        updateContactButton.Text = "Обновить контакт";
        updateContactButton.UseVisualStyleBackColor = true;
        // 
        // createPrivateChatButton
        // 
        usersLayout.SetColumnSpan(createPrivateChatButton, 2);
        createPrivateChatButton.Dock = DockStyle.Fill;
        createPrivateChatButton.Location = new Point(6, 415);
        createPrivateChatButton.Margin = new Padding(6);
        createPrivateChatButton.Name = "createPrivateChatButton";
        createPrivateChatButton.Size = new Size(376, 30);
        createPrivateChatButton.TabIndex = 6;
        createPrivateChatButton.Text = "Создать личный чат";
        createPrivateChatButton.UseVisualStyleBackColor = true;
        // 
        // groupGroup
        // 
        groupGroup.Controls.Add(groupLayout);
        groupGroup.Dock = DockStyle.Fill;
        groupGroup.Location = new Point(8, 507);
        groupGroup.Margin = new Padding(8);
        groupGroup.Name = "groupGroup";
        groupGroup.Padding = new Padding(8);
        groupGroup.Size = new Size(404, 219);
        groupGroup.TabIndex = 1;
        groupGroup.TabStop = false;
        groupGroup.Text = "Групповой чат";
        // 
        // groupLayout
        // 
        groupLayout.ColumnCount = 1;
        groupLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        groupLayout.Controls.Add(groupNameTextBox, 0, 0);
        groupLayout.Controls.Add(groupMembersTextBox, 0, 1);
        groupLayout.Controls.Add(createGroupChatButton, 0, 2);
        groupLayout.Dock = DockStyle.Fill;
        groupLayout.Location = new Point(8, 24);
        groupLayout.Name = "groupLayout";
        groupLayout.RowCount = 3;
        groupLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        groupLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        groupLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        groupLayout.Size = new Size(388, 187);
        groupLayout.TabIndex = 0;
        // 
        // groupNameTextBox
        // 
        groupNameTextBox.Dock = DockStyle.Fill;
        groupNameTextBox.Location = new Point(6, 6);
        groupNameTextBox.Margin = new Padding(6);
        groupNameTextBox.Name = "groupNameTextBox";
        groupNameTextBox.PlaceholderText = "название группы";
        groupNameTextBox.Size = new Size(376, 27);
        groupNameTextBox.TabIndex = 0;
        groupNameTextBox.Text = "TestChat";
        // 
        // groupMembersTextBox
        // 
        groupMembersTextBox.Dock = DockStyle.Fill;
        groupMembersTextBox.Location = new Point(6, 48);
        groupMembersTextBox.Margin = new Padding(6);
        groupMembersTextBox.Multiline = true;
        groupMembersTextBox.Name = "groupMembersTextBox";
        groupMembersTextBox.PlaceholderText = "участники через запятую: stass, stasss";
        groupMembersTextBox.Size = new Size(376, 91);
        groupMembersTextBox.TabIndex = 1;
        groupMembersTextBox.Text = "stass, stasss";
        // 
        // createGroupChatButton
        // 
        createGroupChatButton.Dock = DockStyle.Fill;
        createGroupChatButton.Location = new Point(6, 151);
        createGroupChatButton.Margin = new Padding(6);
        createGroupChatButton.Name = "createGroupChatButton";
        createGroupChatButton.Size = new Size(376, 30);
        createGroupChatButton.TabIndex = 2;
        createGroupChatButton.Text = "Создать групповой чат";
        createGroupChatButton.UseVisualStyleBackColor = true;
        // 
        // centerSplit
        // 
        centerSplit.Dock = DockStyle.Fill;
        centerSplit.Location = new Point(0, 0);
        centerSplit.Name = "centerSplit";
        // 
        // centerSplit.Panel1
        // 
        centerSplit.Panel1.Controls.Add(chatsGroup);
        // 
        // centerSplit.Panel2
        // 
        centerSplit.Panel2.Controls.Add(messagesGroup);
        centerSplit.Panel2.Controls.Add(logGroup);
        centerSplit.Size = new Size(1100, 734);
        centerSplit.SplitterDistance = 360;
        centerSplit.TabIndex = 0;
        // 
        // chatsGroup
        // 
        chatsGroup.Controls.Add(chatsLayout);
        chatsGroup.Dock = DockStyle.Fill;
        chatsGroup.Location = new Point(0, 0);
        chatsGroup.Name = "chatsGroup";
        chatsGroup.Padding = new Padding(8);
        chatsGroup.Size = new Size(360, 734);
        chatsGroup.TabIndex = 0;
        chatsGroup.TabStop = false;
        chatsGroup.Text = "Чаты";
        // 
        // chatsLayout
        // 
        chatsLayout.ColumnCount = 1;
        chatsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        chatsLayout.Controls.Add(refreshChatsButton, 0, 0);
        chatsLayout.Controls.Add(chatsListBox, 0, 1);
        chatsLayout.Controls.Add(participantsButton, 0, 2);
        chatsLayout.Controls.Add(markReadButton, 0, 3);
        chatsLayout.Controls.Add(leaveChatButton, 0, 4);
        chatsLayout.Controls.Add(participantsListBox, 0, 5);
        chatsLayout.Dock = DockStyle.Fill;
        chatsLayout.Location = new Point(8, 24);
        chatsLayout.Name = "chatsLayout";
        chatsLayout.RowCount = 6;
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
        chatsLayout.Size = new Size(344, 702);
        chatsLayout.TabIndex = 0;
        // 
        // refreshChatsButton
        // 
        refreshChatsButton.Dock = DockStyle.Fill;
        refreshChatsButton.Location = new Point(6, 6);
        refreshChatsButton.Margin = new Padding(6);
        refreshChatsButton.Name = "refreshChatsButton";
        refreshChatsButton.Size = new Size(332, 30);
        refreshChatsButton.TabIndex = 0;
        refreshChatsButton.Text = "Обновить чаты";
        refreshChatsButton.UseVisualStyleBackColor = true;
        // 
        // chatsListBox
        // 
        chatsListBox.Dock = DockStyle.Fill;
        chatsListBox.FormattingEnabled = true;
        chatsListBox.Location = new Point(6, 48);
        chatsListBox.Margin = new Padding(6);
        chatsListBox.Name = "chatsListBox";
        chatsListBox.Size = new Size(332, 291);
        chatsListBox.TabIndex = 1;
        // 
        // participantsButton
        // 
        participantsButton.Dock = DockStyle.Fill;
        participantsButton.Location = new Point(6, 351);
        participantsButton.Margin = new Padding(6);
        participantsButton.Name = "participantsButton";
        participantsButton.Size = new Size(332, 30);
        participantsButton.TabIndex = 2;
        participantsButton.Text = "Участники чата";
        participantsButton.UseVisualStyleBackColor = true;
        // 
        // markReadButton
        // 
        markReadButton.Dock = DockStyle.Fill;
        markReadButton.Location = new Point(6, 393);
        markReadButton.Margin = new Padding(6);
        markReadButton.Name = "markReadButton";
        markReadButton.Size = new Size(332, 30);
        markReadButton.TabIndex = 3;
        markReadButton.Text = "Отметить прочитанным";
        markReadButton.UseVisualStyleBackColor = true;
        // 
        // leaveChatButton
        // 
        leaveChatButton.Dock = DockStyle.Fill;
        leaveChatButton.Location = new Point(6, 435);
        leaveChatButton.Margin = new Padding(6);
        leaveChatButton.Name = "leaveChatButton";
        leaveChatButton.Size = new Size(332, 30);
        leaveChatButton.TabIndex = 4;
        leaveChatButton.Text = "Выйти из чата";
        leaveChatButton.UseVisualStyleBackColor = true;
        // 
        // participantsListBox
        // 
        participantsListBox.Dock = DockStyle.Fill;
        participantsListBox.FormattingEnabled = true;
        participantsListBox.Location = new Point(6, 477);
        participantsListBox.Margin = new Padding(6);
        participantsListBox.Name = "participantsListBox";
        participantsListBox.Size = new Size(332, 219);
        participantsListBox.TabIndex = 5;
        // 
        // messagesGroup
        // 
        messagesGroup.Controls.Add(messagesLayout);
        messagesGroup.Dock = DockStyle.Fill;
        messagesGroup.Location = new Point(0, 0);
        messagesGroup.Name = "messagesGroup";
        messagesGroup.Padding = new Padding(8);
        messagesGroup.Size = new Size(736, 514);
        messagesGroup.TabIndex = 0;
        messagesGroup.TabStop = false;
        messagesGroup.Text = "Сообщения";
        // 
        // messagesLayout
        // 
        messagesLayout.ColumnCount = 1;
        messagesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        messagesLayout.Controls.Add(messagesListBox, 0, 0);
        messagesLayout.Controls.Add(messageTextBox, 0, 1);
        messagesLayout.Controls.Add(sendMessageButton, 0, 2);
        messagesLayout.Dock = DockStyle.Fill;
        messagesLayout.Location = new Point(8, 24);
        messagesLayout.Name = "messagesLayout";
        messagesLayout.RowCount = 3;
        messagesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        messagesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        messagesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        messagesLayout.Size = new Size(720, 482);
        messagesLayout.TabIndex = 0;
        // 
        // messagesListBox
        // 
        messagesListBox.Dock = DockStyle.Fill;
        messagesListBox.FormattingEnabled = true;
        messagesListBox.Location = new Point(6, 6);
        messagesListBox.Margin = new Padding(6);
        messagesListBox.Name = "messagesListBox";
        messagesListBox.Size = new Size(708, 386);
        messagesListBox.TabIndex = 0;
        // 
        // messageTextBox
        // 
        messageTextBox.Dock = DockStyle.Fill;
        messageTextBox.Location = new Point(6, 404);
        messageTextBox.Margin = new Padding(6);
        messageTextBox.Name = "messageTextBox";
        messageTextBox.PlaceholderText = "текст сообщения";
        messageTextBox.Size = new Size(708, 27);
        messageTextBox.TabIndex = 1;
        // 
        // sendMessageButton
        // 
        sendMessageButton.Dock = DockStyle.Fill;
        sendMessageButton.Location = new Point(6, 446);
        sendMessageButton.Margin = new Padding(6);
        sendMessageButton.Name = "sendMessageButton";
        sendMessageButton.Size = new Size(708, 30);
        sendMessageButton.TabIndex = 2;
        sendMessageButton.Text = "Отправить сообщение";
        sendMessageButton.UseVisualStyleBackColor = true;
        // 
        // logGroup
        // 
        logGroup.Controls.Add(logLayout);
        logGroup.Dock = DockStyle.Bottom;
        logGroup.Location = new Point(0, 514);
        logGroup.Name = "logGroup";
        logGroup.Padding = new Padding(8);
        logGroup.Size = new Size(736, 220);
        logGroup.TabIndex = 1;
        logGroup.TabStop = false;
        logGroup.Text = "События";
        // 
        // logLayout
        // 
        logLayout.ColumnCount = 1;
        logLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        logLayout.Controls.Add(simulateReconnectButton, 0, 0);
        logLayout.Controls.Add(logListBox, 0, 1);
        logLayout.Dock = DockStyle.Fill;
        logLayout.Location = new Point(8, 24);
        logLayout.Name = "logLayout";
        logLayout.RowCount = 2;
        logLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        logLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        logLayout.Size = new Size(720, 188);
        logLayout.TabIndex = 0;
        // 
        // simulateReconnectButton
        // 
        simulateReconnectButton.Dock = DockStyle.Fill;
        simulateReconnectButton.Location = new Point(6, 6);
        simulateReconnectButton.Margin = new Padding(6);
        simulateReconnectButton.Name = "simulateReconnectButton";
        simulateReconnectButton.Size = new Size(708, 30);
        simulateReconnectButton.TabIndex = 0;
        simulateReconnectButton.Text = "Сымитировать переподключение";
        simulateReconnectButton.UseVisualStyleBackColor = true;
        // 
        // logListBox
        // 
        logListBox.Dock = DockStyle.Fill;
        logListBox.FormattingEnabled = true;
        logListBox.Location = new Point(6, 48);
        logListBox.Margin = new Padding(6);
        logListBox.Name = "logListBox";
        logListBox.Size = new Size(708, 134);
        logListBox.TabIndex = 1;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1540, 900);
        Controls.Add(rootLayout);
        MinimumSize = new Size(1300, 800);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Simple Messenger UI";
        rootLayout.ResumeLayout(false);
        authGroup.ResumeLayout(false);
        authLayout.ResumeLayout(false);
        authLayout.PerformLayout();
        bodySplit.Panel1.ResumeLayout(false);
        bodySplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)bodySplit).EndInit();
        bodySplit.ResumeLayout(false);
        leftLayout.ResumeLayout(false);
        usersGroup.ResumeLayout(false);
        usersLayout.ResumeLayout(false);
        usersLayout.PerformLayout();
        groupGroup.ResumeLayout(false);
        groupLayout.ResumeLayout(false);
        groupLayout.PerformLayout();
        centerSplit.Panel1.ResumeLayout(false);
        centerSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)centerSplit).EndInit();
        centerSplit.ResumeLayout(false);
        chatsGroup.ResumeLayout(false);
        chatsLayout.ResumeLayout(false);
        messagesGroup.ResumeLayout(false);
        messagesLayout.ResumeLayout(false);
        messagesLayout.PerformLayout();
        logGroup.ResumeLayout(false);
        logLayout.ResumeLayout(false);
        ResumeLayout(false);
    }
}
