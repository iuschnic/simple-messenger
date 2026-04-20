using AutoFixture.Xunit2;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Realtime.API.Utils;
using Realtime.BL.InputPorts;
using IGroupManager = Microsoft.AspNetCore.SignalR.IGroupManager;

namespace Realtime.UnitTests;

public class GroupManagerTests
{
    private readonly Mock<IHttpChatReceiver> _mockChatReceiver;
    private readonly Mock<IHubClients> _mockHubClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<IGroupManager> _mockSignalRGroupManager;
    private readonly GroupManager _groupManager;

    public GroupManagerTests()
    {
        var mockHubContext = new Mock<IHubContext<RealtimeHub>>();
        _mockChatReceiver = new Mock<IHttpChatReceiver>();
        _mockHubClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockSignalRGroupManager = new Mock<IGroupManager>();

        mockHubContext
            .Setup(x => x.Clients)
            .Returns(_mockHubClients.Object);

        mockHubContext
            .Setup(x => x.Groups)
            .Returns(_mockSignalRGroupManager.Object);

        _groupManager = new GroupManager(mockHubContext.Object, _mockChatReceiver.Object);
    }

    [Theory, AutoData]
    public async Task SendToGroupAsync_WhenCalled_CallsHubContextGroupSendAsync(
        string groupName, 
        string eventType, 
        string messageJson)
    {
        // Arrange
        _mockHubClients
            .Setup(x => x.Group(groupName))
            .Returns(_mockClientProxy.Object);

        // Act
        await _groupManager.SendToGroupAsync(groupName, eventType, messageJson);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                eventType,
                It.Is<object[]>(o => o.Length == 1 && (string)o[0] == messageJson),
                CancellationToken.None),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task SendToAllUserGroupsAsync_WhenUserHasChats_SendsToAllChatGroups(
        string userId, 
        string eventType, 
        string messageJson)
    {
        // Arrange
        var chatIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        
        _mockChatReceiver
            .Setup(x => x.GetAllChatsByUserIdAsync(userId))
            .ReturnsAsync(chatIds);

        foreach (var chatId in chatIds)
        {
            _mockHubClients
                .Setup(x => x.Group(chatId.ToString()))
                .Returns(_mockClientProxy.Object);
        }

        // Act
        await _groupManager.SendToAllUserGroupsAsync(userId, eventType, messageJson);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                eventType,
                It.Is<object[]>(o => o.Length == 1 && (string)o[0] == messageJson),
                CancellationToken.None),
            Times.Exactly(chatIds.Length));
    }

    [Theory, AutoData]
    public async Task SendToAllUserGroupsAsync_WhenUserHasNoChats_DoesNotSendAnyMessages(
        string userId, 
        string eventType, 
        string messageJson)
    {
        // Arrange
        _mockChatReceiver
            .Setup(x => x.GetAllChatsByUserIdAsync(userId))
            .ReturnsAsync(new List<Guid>());

        // Act
        await _groupManager.SendToAllUserGroupsAsync(userId, eventType, messageJson);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                CancellationToken.None),
            Times.Never);
    }

    [Theory, AutoData]
    public async Task AddToGroupAsync_WhenCalled_CallsSignalRAddToGroupAsync(
        string connectionId, 
        string groupName)
    {
        // Arrange
        _mockSignalRGroupManager
            .Setup(x => x.AddToGroupAsync(connectionId, groupName, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        await _groupManager.AddToGroupAsync(connectionId, groupName);

        // Assert
        _mockSignalRGroupManager.Verify(
            x => x.AddToGroupAsync(connectionId, groupName, CancellationToken.None),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task RemoveFromGroupAsync_WhenCalled_CallsSignalRRemoveFromGroupAsync(
        string connectionId, 
        string groupName)
    {
        // Arrange
        _mockSignalRGroupManager
            .Setup(x => x.RemoveFromGroupAsync(connectionId, groupName, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        await _groupManager.RemoveFromGroupAsync(connectionId, groupName);

        // Assert
        _mockSignalRGroupManager.Verify(
            x => x.RemoveFromGroupAsync(connectionId, groupName, CancellationToken.None),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task AddToGroupAsync_WhenCalled_CompletesSuccessfully(
        string connectionId, 
        string groupName)
    {
        // Arrange
        _mockSignalRGroupManager
            .Setup(x => x.AddToGroupAsync(connectionId, groupName, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var task = _groupManager.AddToGroupAsync(connectionId, groupName);
        await task;

        // Assert
        Assert.True(task.IsCompletedSuccessfully);
    }

    [Theory, AutoData]
    public async Task RemoveFromGroupAsync_WhenCalled_CompletesSuccessfully(
        string connectionId, 
        string groupName)
    {
        // Arrange
        _mockSignalRGroupManager
            .Setup(x => x.RemoveFromGroupAsync(connectionId, groupName, CancellationToken.None))
            .Returns(Task.CompletedTask);

        // Act
        var task = _groupManager.RemoveFromGroupAsync(connectionId, groupName);
        await task;

        // Assert
        Assert.True(task.IsCompletedSuccessfully);
    }
}