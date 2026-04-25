#nullable disable

namespace UI;

partial class MessengerForm
{
    private System.ComponentModel.IContainer components = null;

    private TableLayoutPanel rootLayout;
    private Panel headerPanel;
    private Label headerTitleLabel;
    private Label currentUserLabel;
    private Button refreshChatsButton;
    private Button simulateReconnectButton;
    private SplitContainer mainSplit;
    private TableLayoutPanel leftLayout;
    private GroupBox usersGroup;
    private TableLayoutPanel usersLayout;
    private TextBox searchUserTextBox;
    private Button searchUserButton;
    private Button loadContactsButton;
    private ListBox usersListBox;
    private TextBox contactNameTextBox;
    private Button updateContactButton;
    private Button createPrivateChatButton;
    private GroupBox groupGroup;
    private TableLayoutPanel groupLayout;
    private TextBox groupNameTextBox;
    private CheckedListBox groupMembersCheckedListBox;
    private Button refreshGroupMembersButton;
    private Button createGroupChatButton;
    private SplitContainer rightSplit;
    private GroupBox chatsGroup;
    private TableLayoutPanel chatsLayout;
    private ListBox chatsListBox;
    private Button participantsButton;
    private Button markReadButton;
    private Button leaveChatButton;
    private ListBox participantsListBox;
    private GroupBox messagesGroup;
    private TableLayoutPanel messagesLayout;
    private Label activeChatLabel;
    private ListBox messagesListBox;
    private TextBox messageTextBox;
    private Button sendMessageButton;
    private GroupBox logGroup;
    private ListBox logListBox;

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
        headerPanel = new Panel();
        headerTitleLabel = new Label();
        currentUserLabel = new Label();
        refreshChatsButton = new Button();
        simulateReconnectButton = new Button();
        mainSplit = new SplitContainer();
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
        groupMembersCheckedListBox = new CheckedListBox();
        refreshGroupMembersButton = new Button();
        createGroupChatButton = new Button();
        rightSplit = new SplitContainer();
        chatsGroup = new GroupBox();
        chatsLayout = new TableLayoutPanel();
        chatsListBox = new ListBox();
        participantsButton = new Button();
        markReadButton = new Button();
        leaveChatButton = new Button();
        participantsListBox = new ListBox();
        messagesGroup = new GroupBox();
        messagesLayout = new TableLayoutPanel();
        activeChatLabel = new Label();
        messagesListBox = new ListBox();
        messageTextBox = new TextBox();
        sendMessageButton = new Button();
        logGroup = new GroupBox();
        logListBox = new ListBox();
        rootLayout.SuspendLayout();
        headerPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)mainSplit).BeginInit();
        mainSplit.Panel1.SuspendLayout();
        mainSplit.Panel2.SuspendLayout();
        mainSplit.SuspendLayout();
        leftLayout.SuspendLayout();
        usersGroup.SuspendLayout();
        usersLayout.SuspendLayout();
        groupGroup.SuspendLayout();
        groupLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)rightSplit).BeginInit();
        rightSplit.Panel1.SuspendLayout();
        rightSplit.Panel2.SuspendLayout();
        rightSplit.SuspendLayout();
        chatsGroup.SuspendLayout();
        chatsLayout.SuspendLayout();
        messagesGroup.SuspendLayout();
        messagesLayout.SuspendLayout();
        logGroup.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayout
        // 
        rootLayout.BackColor = Color.FromArgb(240, 244, 248);
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(headerPanel, 0, 0);
        rootLayout.Controls.Add(mainSplit, 0, 1);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Location = new Point(0, 0);
        rootLayout.Name = "rootLayout";
        rootLayout.RowCount = 2;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.Size = new Size(1540, 900);
        rootLayout.TabIndex = 0;
        // 
        // headerPanel
        // 
        headerPanel.BackColor = Color.FromArgb(24, 40, 72);
        headerPanel.Controls.Add(simulateReconnectButton);
        headerPanel.Controls.Add(refreshChatsButton);
        headerPanel.Controls.Add(currentUserLabel);
        headerPanel.Controls.Add(headerTitleLabel);
        headerPanel.Dock = DockStyle.Fill;
        headerPanel.Location = new Point(0, 0);
        headerPanel.Margin = new Padding(0);
        headerPanel.Name = "headerPanel";
        headerPanel.Size = new Size(1540, 76);
        headerPanel.TabIndex = 0;
        // 
        // headerTitleLabel
        // 
        headerTitleLabel.AutoSize = true;
        headerTitleLabel.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold);
        headerTitleLabel.ForeColor = Color.White;
        headerTitleLabel.Location = new Point(20, 18);
        headerTitleLabel.Name = "headerTitleLabel";
        headerTitleLabel.Size = new Size(129, 41);
        headerTitleLabel.TabIndex = 0;
        headerTitleLabel.Text = "Чаты";
        // 
        // currentUserLabel
        // 
        currentUserLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        currentUserLabel.ForeColor = Color.FromArgb(220, 227, 235);
        currentUserLabel.Location = new Point(799, 23);
        currentUserLabel.Name = "currentUserLabel";
        currentUserLabel.Size = new Size(420, 30);
        currentUserLabel.TabIndex = 1;
        currentUserLabel.Text = "Текущий пользователь";
        currentUserLabel.TextAlign = ContentAlignment.MiddleRight;
        // 
        // refreshChatsButton
        // 
        refreshChatsButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        refreshChatsButton.Location = new Point(1235, 18);
        refreshChatsButton.Name = "refreshChatsButton";
        refreshChatsButton.Size = new Size(137, 40);
        refreshChatsButton.TabIndex = 2;
        refreshChatsButton.Text = "Обновить";
        refreshChatsButton.UseVisualStyleBackColor = true;
        // 
        // simulateReconnectButton
        // 
        simulateReconnectButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        simulateReconnectButton.Location = new Point(1383, 18);
        simulateReconnectButton.Name = "simulateReconnectButton";
        simulateReconnectButton.Size = new Size(137, 40);
        simulateReconnectButton.TabIndex = 3;
        simulateReconnectButton.Text = "Переподключ.";
        simulateReconnectButton.UseVisualStyleBackColor = true;
        // 
        // mainSplit
        // 
        mainSplit.Dock = DockStyle.Fill;
        mainSplit.FixedPanel = FixedPanel.Panel1;
        mainSplit.Location = new Point(10, 86);
        mainSplit.Margin = new Padding(10);
        mainSplit.Name = "mainSplit";
        // 
        // mainSplit.Panel1
        // 
        mainSplit.Panel1.Controls.Add(leftLayout);
        // 
        // mainSplit.Panel2
        // 
        mainSplit.Panel2.Controls.Add(rightSplit);
        mainSplit.Size = new Size(1520, 804);
        mainSplit.SplitterDistance = 390;
        mainSplit.TabIndex = 1;
        // 
        // leftLayout
        // 
        leftLayout.ColumnCount = 1;
        leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        leftLayout.Controls.Add(usersGroup, 0, 0);
        leftLayout.Controls.Add(groupGroup, 0, 1);
        leftLayout.Dock = DockStyle.Fill;
        leftLayout.Location = new Point(0, 0);
        leftLayout.Name = "leftLayout";
        leftLayout.RowCount = 2;
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 64F));
        leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 36F));
        leftLayout.Size = new Size(390, 804);
        leftLayout.TabIndex = 0;
        // 
        // usersGroup
        // 
        usersGroup.Controls.Add(usersLayout);
        usersGroup.Dock = DockStyle.Fill;
        usersGroup.Location = new Point(0, 0);
        usersGroup.Margin = new Padding(0, 0, 0, 10);
        usersGroup.Name = "usersGroup";
        usersGroup.Padding = new Padding(10);
        usersGroup.Size = new Size(390, 504);
        usersGroup.TabIndex = 0;
        usersGroup.TabStop = false;
        usersGroup.Text = "Пользователи";
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
        usersLayout.Location = new Point(10, 30);
        usersLayout.Name = "usersLayout";
        usersLayout.RowCount = 5;
        usersLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        usersLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        usersLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        usersLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        usersLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        usersLayout.Size = new Size(370, 464);
        usersLayout.TabIndex = 0;
        // 
        // searchUserTextBox
        // 
        searchUserTextBox.Dock = DockStyle.Fill;
        searchUserTextBox.Location = new Point(6, 6);
        searchUserTextBox.Margin = new Padding(6);
        searchUserTextBox.Name = "searchUserTextBox";
        searchUserTextBox.PlaceholderText = "Найти пользователя";
        searchUserTextBox.Size = new Size(173, 27);
        searchUserTextBox.TabIndex = 0;
        searchUserTextBox.Text = "stass";
        // 
        // searchUserButton
        // 
        searchUserButton.Dock = DockStyle.Fill;
        searchUserButton.Location = new Point(191, 6);
        searchUserButton.Margin = new Padding(6);
        searchUserButton.Name = "searchUserButton";
        searchUserButton.Size = new Size(173, 32);
        searchUserButton.TabIndex = 1;
        searchUserButton.Text = "Найти";
        searchUserButton.UseVisualStyleBackColor = true;
        // 
        // loadContactsButton
        // 
        usersLayout.SetColumnSpan(loadContactsButton, 2);
        loadContactsButton.Dock = DockStyle.Fill;
        loadContactsButton.Location = new Point(6, 50);
        loadContactsButton.Margin = new Padding(6);
        loadContactsButton.Name = "loadContactsButton";
        loadContactsButton.Size = new Size(358, 32);
        loadContactsButton.TabIndex = 2;
        loadContactsButton.Text = "Показать контакты";
        loadContactsButton.UseVisualStyleBackColor = true;
        // 
        // usersListBox
        // 
        usersLayout.SetColumnSpan(usersListBox, 2);
        usersListBox.Dock = DockStyle.Fill;
        usersListBox.FormattingEnabled = true;
        usersListBox.Location = new Point(6, 94);
        usersListBox.Margin = new Padding(6);
        usersListBox.Name = "usersListBox";
        usersListBox.Size = new Size(358, 272);
        usersListBox.TabIndex = 3;
        // 
        // contactNameTextBox
        // 
        contactNameTextBox.Dock = DockStyle.Fill;
        contactNameTextBox.Location = new Point(6, 378);
        contactNameTextBox.Margin = new Padding(6);
        contactNameTextBox.Name = "contactNameTextBox";
        contactNameTextBox.PlaceholderText = "Имя контакта";
        contactNameTextBox.Size = new Size(173, 27);
        contactNameTextBox.TabIndex = 4;
        // 
        // updateContactButton
        // 
        updateContactButton.Dock = DockStyle.Fill;
        updateContactButton.Location = new Point(191, 378);
        updateContactButton.Margin = new Padding(6);
        updateContactButton.Name = "updateContactButton";
        updateContactButton.Size = new Size(173, 32);
        updateContactButton.TabIndex = 5;
        updateContactButton.Text = "Сохранить";
        updateContactButton.UseVisualStyleBackColor = true;
        // 
        // createPrivateChatButton
        // 
        usersLayout.SetColumnSpan(createPrivateChatButton, 2);
        createPrivateChatButton.Dock = DockStyle.Fill;
        createPrivateChatButton.Location = new Point(6, 422);
        createPrivateChatButton.Margin = new Padding(6);
        createPrivateChatButton.Name = "createPrivateChatButton";
        createPrivateChatButton.Size = new Size(358, 36);
        createPrivateChatButton.TabIndex = 6;
        createPrivateChatButton.Text = "Создать личный чат";
        createPrivateChatButton.UseVisualStyleBackColor = true;
        // 
        // groupGroup
        // 
        groupGroup.Controls.Add(groupLayout);
        groupGroup.Dock = DockStyle.Fill;
        groupGroup.Location = new Point(0, 524);
        groupGroup.Margin = new Padding(0);
        groupGroup.Name = "groupGroup";
        groupGroup.Padding = new Padding(10);
        groupGroup.Size = new Size(390, 280);
        groupGroup.TabIndex = 1;
        groupGroup.TabStop = false;
        groupGroup.Text = "Новый групповой чат";
        // 
        // groupLayout
        // 
        groupLayout.ColumnCount = 1;
        groupLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        groupLayout.Controls.Add(groupNameTextBox, 0, 0);
        groupLayout.Controls.Add(refreshGroupMembersButton, 0, 1);
        groupLayout.Controls.Add(groupMembersCheckedListBox, 0, 2);
        groupLayout.Controls.Add(createGroupChatButton, 0, 3);
        groupLayout.Dock = DockStyle.Fill;
        groupLayout.Location = new Point(10, 30);
        groupLayout.Name = "groupLayout";
        groupLayout.RowCount = 4;
        groupLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        groupLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        groupLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        groupLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        groupLayout.Size = new Size(370, 240);
        groupLayout.TabIndex = 0;
        // 
        // groupNameTextBox
        // 
        groupNameTextBox.Dock = DockStyle.Fill;
        groupNameTextBox.Location = new Point(6, 6);
        groupNameTextBox.Margin = new Padding(6);
        groupNameTextBox.Name = "groupNameTextBox";
        groupNameTextBox.PlaceholderText = "Название чата";
        groupNameTextBox.Size = new Size(358, 27);
        groupNameTextBox.TabIndex = 0;
        groupNameTextBox.Text = "TestChat";
        // 
        // refreshGroupMembersButton
        // 
        refreshGroupMembersButton.Dock = DockStyle.Fill;
        refreshGroupMembersButton.Location = new Point(6, 50);
        refreshGroupMembersButton.Margin = new Padding(6);
        refreshGroupMembersButton.Name = "refreshGroupMembersButton";
        refreshGroupMembersButton.Size = new Size(358, 32);
        refreshGroupMembersButton.TabIndex = 1;
        refreshGroupMembersButton.Text = "Обновить список контактов";
        refreshGroupMembersButton.UseVisualStyleBackColor = true;
        // 
        // groupMembersCheckedListBox
        // 
        groupMembersCheckedListBox.CheckOnClick = true;
        groupMembersCheckedListBox.Dock = DockStyle.Fill;
        groupMembersCheckedListBox.FormattingEnabled = true;
        groupMembersCheckedListBox.Location = new Point(6, 94);
        groupMembersCheckedListBox.Margin = new Padding(6);
        groupMembersCheckedListBox.Name = "groupMembersCheckedListBox";
        groupMembersCheckedListBox.Size = new Size(358, 92);
        groupMembersCheckedListBox.TabIndex = 2;
        // 
        // createGroupChatButton
        // 
        createGroupChatButton.Dock = DockStyle.Fill;
        createGroupChatButton.Location = new Point(6, 198);
        createGroupChatButton.Margin = new Padding(6);
        createGroupChatButton.Name = "createGroupChatButton";
        createGroupChatButton.Size = new Size(358, 36);
        createGroupChatButton.TabIndex = 3;
        createGroupChatButton.Text = "Создать групповой чат";
        createGroupChatButton.UseVisualStyleBackColor = true;
        // 
        // rightSplit
        // 
        rightSplit.Dock = DockStyle.Fill;
        rightSplit.Location = new Point(0, 0);
        rightSplit.Name = "rightSplit";
        // 
        // rightSplit.Panel1
        // 
        rightSplit.Panel1.Controls.Add(chatsGroup);
        // 
        // rightSplit.Panel2
        // 
        rightSplit.Panel2.Controls.Add(messagesGroup);
        rightSplit.Panel2.Controls.Add(logGroup);
        rightSplit.Size = new Size(1126, 804);
        rightSplit.SplitterDistance = 320;
        rightSplit.TabIndex = 0;
        // 
        // chatsGroup
        // 
        chatsGroup.Controls.Add(chatsLayout);
        chatsGroup.Dock = DockStyle.Fill;
        chatsGroup.Location = new Point(0, 0);
        chatsGroup.Name = "chatsGroup";
        chatsGroup.Padding = new Padding(10);
        chatsGroup.Size = new Size(320, 804);
        chatsGroup.TabIndex = 0;
        chatsGroup.TabStop = false;
        chatsGroup.Text = "Мои чаты";
        // 
        // chatsLayout
        // 
        chatsLayout.ColumnCount = 1;
        chatsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        chatsLayout.Controls.Add(chatsListBox, 0, 0);
        chatsLayout.Controls.Add(participantsButton, 0, 1);
        chatsLayout.Controls.Add(markReadButton, 0, 2);
        chatsLayout.Controls.Add(leaveChatButton, 0, 3);
        chatsLayout.Controls.Add(participantsListBox, 0, 4);
        chatsLayout.Dock = DockStyle.Fill;
        chatsLayout.Location = new Point(10, 30);
        chatsLayout.Name = "chatsLayout";
        chatsLayout.RowCount = 5;
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        chatsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
        chatsLayout.Size = new Size(300, 764);
        chatsLayout.TabIndex = 0;
        // 
        // chatsListBox
        // 
        chatsListBox.Dock = DockStyle.Fill;
        chatsListBox.FormattingEnabled = true;
        chatsListBox.Location = new Point(6, 6);
        chatsListBox.Margin = new Padding(6);
        chatsListBox.Name = "chatsListBox";
        chatsListBox.Size = new Size(288, 353);
        chatsListBox.TabIndex = 0;
        // 
        // participantsButton
        // 
        participantsButton.Dock = DockStyle.Fill;
        participantsButton.Location = new Point(6, 371);
        participantsButton.Margin = new Padding(6);
        participantsButton.Name = "participantsButton";
        participantsButton.Size = new Size(288, 32);
        participantsButton.TabIndex = 1;
        participantsButton.Text = "Обновить участников";
        participantsButton.UseVisualStyleBackColor = true;
        // 
        // markReadButton
        // 
        markReadButton.Dock = DockStyle.Fill;
        markReadButton.Location = new Point(6, 415);
        markReadButton.Margin = new Padding(6);
        markReadButton.Name = "markReadButton";
        markReadButton.Size = new Size(288, 32);
        markReadButton.TabIndex = 2;
        markReadButton.Text = "Отметить прочитанным";
        markReadButton.UseVisualStyleBackColor = true;
        // 
        // leaveChatButton
        // 
        leaveChatButton.Dock = DockStyle.Fill;
        leaveChatButton.Location = new Point(6, 459);
        leaveChatButton.Margin = new Padding(6);
        leaveChatButton.Name = "leaveChatButton";
        leaveChatButton.Size = new Size(288, 32);
        leaveChatButton.TabIndex = 3;
        leaveChatButton.Text = "Выйти из чата";
        leaveChatButton.UseVisualStyleBackColor = true;
        // 
        // participantsListBox
        // 
        participantsListBox.Dock = DockStyle.Fill;
        participantsListBox.FormattingEnabled = true;
        participantsListBox.Location = new Point(6, 503);
        participantsListBox.Margin = new Padding(6);
        participantsListBox.Name = "participantsListBox";
        participantsListBox.Size = new Size(288, 255);
        participantsListBox.TabIndex = 4;
        // 
        // messagesGroup
        // 
        messagesGroup.Controls.Add(messagesLayout);
        messagesGroup.Dock = DockStyle.Fill;
        messagesGroup.Location = new Point(0, 0);
        messagesGroup.Name = "messagesGroup";
        messagesGroup.Padding = new Padding(10);
        messagesGroup.Size = new Size(802, 584);
        messagesGroup.TabIndex = 0;
        messagesGroup.TabStop = false;
        messagesGroup.Text = "Диалог";
        // 
        // messagesLayout
        // 
        messagesLayout.ColumnCount = 1;
        messagesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        messagesLayout.Controls.Add(activeChatLabel, 0, 0);
        messagesLayout.Controls.Add(messagesListBox, 0, 1);
        messagesLayout.Controls.Add(messageTextBox, 0, 2);
        messagesLayout.Controls.Add(sendMessageButton, 0, 3);
        messagesLayout.Dock = DockStyle.Fill;
        messagesLayout.Location = new Point(10, 30);
        messagesLayout.Name = "messagesLayout";
        messagesLayout.RowCount = 4;
        messagesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        messagesLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        messagesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        messagesLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        messagesLayout.Size = new Size(782, 544);
        messagesLayout.TabIndex = 0;
        // 
        // activeChatLabel
        // 
        activeChatLabel.Dock = DockStyle.Fill;
        activeChatLabel.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
        activeChatLabel.Location = new Point(3, 0);
        activeChatLabel.Name = "activeChatLabel";
        activeChatLabel.Size = new Size(776, 40);
        activeChatLabel.TabIndex = 0;
        activeChatLabel.Text = "Выберите чат";
        activeChatLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // messagesListBox
        // 
        messagesListBox.Dock = DockStyle.Fill;
        messagesListBox.FormattingEnabled = true;
        messagesListBox.Location = new Point(6, 46);
        messagesListBox.Margin = new Padding(6);
        messagesListBox.Name = "messagesListBox";
        messagesListBox.Size = new Size(770, 394);
        messagesListBox.TabIndex = 1;
        // 
        // messageTextBox
        // 
        messageTextBox.Dock = DockStyle.Fill;
        messageTextBox.Location = new Point(6, 452);
        messageTextBox.Margin = new Padding(6);
        messageTextBox.Name = "messageTextBox";
        messageTextBox.PlaceholderText = "Введите сообщение";
        messageTextBox.Size = new Size(770, 27);
        messageTextBox.TabIndex = 2;
        // 
        // sendMessageButton
        // 
        sendMessageButton.BackColor = Color.FromArgb(39, 84, 138);
        sendMessageButton.Dock = DockStyle.Fill;
        sendMessageButton.FlatStyle = FlatStyle.Flat;
        sendMessageButton.ForeColor = Color.White;
        sendMessageButton.Location = new Point(6, 500);
        sendMessageButton.Margin = new Padding(6);
        sendMessageButton.Name = "sendMessageButton";
        sendMessageButton.Size = new Size(770, 38);
        sendMessageButton.TabIndex = 3;
        sendMessageButton.Text = "Отправить";
        sendMessageButton.UseVisualStyleBackColor = false;
        // 
        // logGroup
        // 
        logGroup.Controls.Add(logListBox);
        logGroup.Dock = DockStyle.Bottom;
        logGroup.Location = new Point(0, 584);
        logGroup.Name = "logGroup";
        logGroup.Padding = new Padding(10);
        logGroup.Size = new Size(802, 220);
        logGroup.TabIndex = 1;
        logGroup.TabStop = false;
        logGroup.Text = "События";
        // 
        // logListBox
        // 
        logListBox.Dock = DockStyle.Fill;
        logListBox.FormattingEnabled = true;
        logListBox.Location = new Point(10, 30);
        logListBox.Name = "logListBox";
        logListBox.Size = new Size(782, 180);
        logListBox.TabIndex = 0;
        // 
        // MessengerForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1540, 900);
        Controls.Add(rootLayout);
        MinimumSize = new Size(1320, 820);
        Name = "MessengerForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Simple Messenger";
        rootLayout.ResumeLayout(false);
        headerPanel.ResumeLayout(false);
        headerPanel.PerformLayout();
        mainSplit.Panel1.ResumeLayout(false);
        mainSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplit).EndInit();
        mainSplit.ResumeLayout(false);
        leftLayout.ResumeLayout(false);
        usersGroup.ResumeLayout(false);
        usersLayout.ResumeLayout(false);
        usersLayout.PerformLayout();
        groupGroup.ResumeLayout(false);
        groupLayout.ResumeLayout(false);
        groupLayout.PerformLayout();
        rightSplit.Panel1.ResumeLayout(false);
        rightSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)rightSplit).EndInit();
        rightSplit.ResumeLayout(false);
        chatsGroup.ResumeLayout(false);
        chatsLayout.ResumeLayout(false);
        messagesGroup.ResumeLayout(false);
        messagesLayout.ResumeLayout(false);
        messagesLayout.PerformLayout();
        logGroup.ResumeLayout(false);
        ResumeLayout(false);
    }
}
