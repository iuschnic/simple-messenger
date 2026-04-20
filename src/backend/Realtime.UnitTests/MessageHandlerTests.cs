using AutoFixture.Xunit2;
using Moq;
using Newtonsoft.Json;
using Realtime.BL;
using Realtime.BL.InputPorts;
using Serilog;
using Shared.Main.Realtime;
using Shared.Main.Realtime.Models;

namespace Realtime.UnitTests;

public class MessageHandlerTests
{
    private readonly Mock<IConnectionsRepository> _mockRepository;
    private readonly Mock<IGroupManager> _mockGroupManager;
    private readonly Mock<ILogger> _mockLogger;
    private readonly MessageHandler _messageHandler;

    public MessageHandlerTests()
    {
        _mockRepository = new Mock<IConnectionsRepository>();
        _mockGroupManager = new Mock<IGroupManager>();
        _mockLogger = new Mock<ILogger>();
        _messageHandler = new MessageHandler(
            _mockRepository.Object,
            _mockGroupManager.Object,
            _mockLogger.Object);
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenMessageReceived_LogsAndSendsToGroup(
        MessageDto message)
    {
        // Arrange
        var dataJson = JsonConvert.SerializeObject(message);
        var eventType = EventType.MessageReceived;

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockLogger.Verify(
            x => x.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson),
            Times.Once);

        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                message.ChatId.ToString(),
                eventType.ToString(),
                dataJson),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenMessageUpdated_LogsAndSendsToGroup(
        MessageDto message)
    {
        // Arrange
        var dataJson = JsonConvert.SerializeObject(message);
        var eventType = EventType.MessageUpdated;

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockLogger.Verify(
            x => x.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson),
            Times.Once);

        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                message.ChatId.ToString(),
                eventType.ToString(),
                dataJson),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenUserChanged_LogsAndSendsToAllUserGroups(
        UserDto user)
    {
        // Arrange
        var dataJson = JsonConvert.SerializeObject(user);
        var eventType = EventType.UserChanged;

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockLogger.Verify(
            x => x.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson),
            Times.Once);

        _mockGroupManager.Verify(
            x => x.SendToAllUserGroupsAsync(
                user.Id.ToString(),
                nameof(EventType.UserChanged),
                dataJson),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenChatUpdated_LogsAndSendsToGroup(
        ChatDto chat)
    {
        // Arrange
        var dataJson = JsonConvert.SerializeObject(chat);
        var eventType = EventType.ChatUpdated;

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockLogger.Verify(
            x => x.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson),
            Times.Once);

        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                chat.Id.ToString(),
                nameof(EventType.ChatUpdated),
                dataJson),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenMessageRead_RemovesUserFromGroupAndSendsToGroup(
        ChatUserDto chatUser)
    {
        // Arrange
        var dataJson = JsonConvert.SerializeObject(chatUser);
        var eventType = EventType.MessageRead;
        var connectionIds = new[] { "conn1", "conn2" };

        _mockRepository
            .Setup(x => x.GetConnectionIdsByUserId(chatUser.User.Id.ToString()))
            .Returns(connectionIds);

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockLogger.Verify(
            x => x.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson),
            Times.Once);

        foreach (var connectionId in connectionIds)
        {
            _mockGroupManager.Verify(
                x => x.RemoveFromGroupAsync(connectionId, chatUser.ChatId.ToString()),
                Times.Once);
        }

        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                chatUser.ChatId.ToString(),
                eventType.ToString(),
                dataJson),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenChatUserJoined_AddsUserToGroupAndSendsToGroup(
        ChatUserDto chatUser)
    {
        // Arrange
        var dataJson = JsonConvert.SerializeObject(chatUser);
        var eventType = EventType.ChatUserJoined;
        var connectionIds = new[] { "conn1", "conn2" };

        _mockRepository
            .Setup(x => x.GetConnectionIdsByUserId(chatUser.User.Id.ToString()))
            .Returns(connectionIds);

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockLogger.Verify(
            x => x.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson),
            Times.Once);

        foreach (var connectionId in connectionIds)
        {
            _mockGroupManager.Verify(
                x => x.AddToGroupAsync(connectionId, chatUser.ChatId.ToString()),
                Times.Once);
        }

        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                chatUser.ChatId.ToString(),
                eventType.ToString(),
                dataJson),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenChatUserLeft_RemovesUserFromGroupAndSendsToGroup(
        ChatUserDto chatUser)
    {
        // Arrange
        var dataJson = JsonConvert.SerializeObject(chatUser);
        var eventType = EventType.ChatUserLeft;
        var connectionIds = new[] { "conn1", "conn2" };

        _mockRepository
            .Setup(x => x.GetConnectionIdsByUserId(chatUser.User.Id.ToString()))
            .Returns(connectionIds);

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockLogger.Verify(
            x => x.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson),
            Times.Once);

        foreach (var connectionId in connectionIds)
        {
            _mockGroupManager.Verify(
                x => x.RemoveFromGroupAsync(connectionId, chatUser.ChatId.ToString()),
                Times.Once);
        }

        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                chatUser.ChatId.ToString(),
                eventType.ToString(),
                dataJson),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenChatDeleted_RemovesAllParticipantsAndSendsToGroup(
        FullChatDto fullChat)
    {
        // Arrange
        var dataJson = JsonConvert.SerializeObject(fullChat);
        var eventType = EventType.ChatDeleted;
        var connectionIds = new[] { "conn1", "conn2" };

        foreach (var participant in fullChat.Participants)
        {
            _mockRepository
                .Setup(x => x.GetConnectionIdsByUserId(participant.User.Id.ToString()))
                .Returns(connectionIds);
        }

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockLogger.Verify(
            x => x.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson),
            Times.Once);

        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                fullChat.Chat.Id.ToString(),
                nameof(EventType.ChatDeleted),
                It.Is<string>(s => s.Contains(fullChat.Chat.Id.ToString()))),
            Times.Once);

        var expectedRemoveCount = fullChat.Participants.Count * connectionIds.Length;
        _mockGroupManager.Verify(
            x => x.RemoveFromGroupAsync(It.IsAny<string>(), fullChat.Chat.Id.ToString()),
            Times.Exactly(expectedRemoveCount));
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenChatCreated_AddsAllParticipantsAndSendsToGroup(
        FullChatDto fullChat)
    {
        // Arrange
        var dataJson = JsonConvert.SerializeObject(fullChat);
        var eventType = EventType.ChatCreated;
        var connectionIds = new[] { "conn1", "conn2" };

        foreach (var participant in fullChat.Participants)
        {
            _mockRepository
                .Setup(x => x.GetConnectionIdsByUserId(participant.User.Id.ToString()))
                .Returns(connectionIds);
        }

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockLogger.Verify(
            x => x.Information("Received message from broker: {EventType} {DataJson}", eventType, dataJson),
            Times.Once);

        var expectedAddCount = fullChat.Participants.Count * connectionIds.Length;
        _mockGroupManager.Verify(
            x => x.AddToGroupAsync(It.IsAny<string>(), fullChat.Chat.Id.ToString()),
            Times.Exactly(expectedAddCount));

        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                fullChat.Chat.Id.ToString(),
                nameof(EventType.ChatCreated),
                dataJson),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenMessageDtoDeserializationFails_ThrowsException(
        EventType eventType)
    {
        // Arrange
        var invalidJson = "invalid json";
        var testEventType = eventType == EventType.MessageReceived || eventType == EventType.MessageUpdated 
            ? eventType 
            : EventType.MessageReceived;

        // Act & Assert
        await Assert.ThrowsAsync<FailedToDeserializeMessageException>(
            () => _messageHandler.OnMessageReceivedFromBrokerAsync(testEventType, invalidJson));
    }

    [Fact]
    public async Task OnMessageReceivedFromBrokerAsync_WhenUserDtoDeserializationFails_ThrowsException()
    {
        // Arrange
        var invalidJson = "invalid json";
        var eventType = EventType.UserChanged;

        // Act & Assert
        await Assert.ThrowsAsync<FailedToDeserializeMessageException>(
            () => _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, invalidJson));
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenChatUserDtoDeserializationFails_ThrowsException(
        EventType eventType)
    {
        // Arrange
        var invalidJson = "invalid json";
        var testEventType = eventType == EventType.MessageRead || 
                           eventType == EventType.ChatUserJoined || 
                           eventType == EventType.ChatUserLeft
            ? eventType
            : EventType.MessageRead;

        // Act & Assert
        await Assert.ThrowsAsync<FailedToDeserializeMessageException>(
            () => _messageHandler.OnMessageReceivedFromBrokerAsync(testEventType, invalidJson));
    }

    [Fact]
    public async Task OnMessageReceivedFromBrokerAsync_WhenChatDtoDeserializationFails_ThrowsException()
    {
        // Arrange
        var invalidJson = "invalid json";
        var eventType = EventType.ChatUpdated;

        // Act & Assert
        await Assert.ThrowsAsync<FailedToDeserializeMessageException>(
            () => _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, invalidJson));
    }

    [Theory, AutoData]
    public async Task OnMessageReceivedFromBrokerAsync_WhenFullChatDtoDeserializationFails_ThrowsException(
        EventType eventType)
    {
        // Arrange
        var invalidJson = "invalid json";
        var testEventType = eventType == EventType.ChatDeleted || eventType == EventType.ChatCreated
            ? eventType
            : EventType.ChatCreated;

        // Act & Assert
        await Assert.ThrowsAsync<FailedToDeserializeMessageException>(
            () => _messageHandler.OnMessageReceivedFromBrokerAsync(testEventType, invalidJson));
    }

    [Fact]
    public async Task OnMessageReceivedFromBrokerAsync_WhenChatDeletedAndParticipantsHaveNoConnections_StillSendsToGroup()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var chat = new ChatDto(
            chatId,
            "Test Chat",
            ChatType.Group,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            1,
            1);

        var participants = new List<ChatUserDto>
        {
            new(chatId, new UserDto(Guid.NewGuid(), "user1", "User 1"), 0),
            new(chatId, new UserDto(Guid.NewGuid(), "user2", "User 2"), 0)
        };

        var fullChat = new FullChatDto(chat, participants);
        var dataJson = JsonConvert.SerializeObject(fullChat);
        var eventType = EventType.ChatDeleted;

        foreach (var participant in fullChat.Participants)
        {
            _mockRepository
                .Setup(x => x.GetConnectionIdsByUserId(participant.User.Id.ToString()))
                .Returns(Enumerable.Empty<string>());
        }

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                fullChat.Chat.Id.ToString(),
                nameof(EventType.ChatDeleted),
                It.IsAny<string>()),
            Times.Once);

        _mockGroupManager.Verify(
            x => x.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task OnMessageReceivedFromBrokerAsync_WhenChatCreatedAndParticipantsHaveNoConnections_StillSendsToGroup()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var chat = new ChatDto(
            chatId,
            "Test Chat",
            ChatType.Group,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            1,
            1);

        var participants = new List<ChatUserDto>
        {
            new(chatId, new UserDto(Guid.NewGuid(), "user1", "User 1"), 0),
            new(chatId, new UserDto(Guid.NewGuid(), "user2", "User 2"), 0)
        };

        var fullChat = new FullChatDto(chat, participants);
        var dataJson = JsonConvert.SerializeObject(fullChat);
        var eventType = EventType.ChatCreated;

        foreach (var participant in fullChat.Participants)
        {
            _mockRepository
                .Setup(x => x.GetConnectionIdsByUserId(participant.User.Id.ToString()))
                .Returns(Enumerable.Empty<string>());
        }

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockGroupManager.Verify(
            x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                fullChat.Chat.Id.ToString(),
                nameof(EventType.ChatCreated),
                dataJson),
            Times.Once);
    }

    [Fact]
    public async Task OnMessageReceivedFromBrokerAsync_WhenChatUserJoinedAndUserHasNoConnections_StillSendsToGroup()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var user = new UserDto(Guid.NewGuid(), "testuser", "Test User");
        var chatUser = new ChatUserDto(chatId, user, 0);
        var dataJson = JsonConvert.SerializeObject(chatUser);
        var eventType = EventType.ChatUserJoined;

        _mockRepository
            .Setup(x => x.GetConnectionIdsByUserId(chatUser.User.Id.ToString()))
            .Returns(Enumerable.Empty<string>());

        // Act
        await _messageHandler.OnMessageReceivedFromBrokerAsync(eventType, dataJson);

        // Assert
        _mockGroupManager.Verify(
            x => x.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _mockGroupManager.Verify(
            x => x.SendToGroupAsync(
                chatUser.ChatId.ToString(),
                eventType.ToString(),
                dataJson),
            Times.Once);
    }
}