using AutoFixture.Xunit2;
using Moq;
using Realtime.BL;
using Realtime.BL.InputPorts;

namespace Realtime.UnitTests;

public class ConnectionManagerTests
{
    private readonly Mock<IConnectionsRepository> _mockRepository;
    private readonly ConnectionManager _connectionManager;

    public ConnectionManagerTests()
    {
        _mockRepository = new Mock<IConnectionsRepository>();
        _connectionManager = new ConnectionManager(_mockRepository.Object);
    }

    [Theory, AutoData]
    public async Task OnUserConnectedAsync_WhenCalled_CallsRepositoryAddUserConnection(
        string userId, 
        string connectionId)
    {
        // Act
        await _connectionManager.OnUserConnectedAsync(userId, connectionId);

        // Assert
        _mockRepository.Verify(
            x => x.AddUserConnection(userId, connectionId),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnUserDisconnectedAsync_WhenCalled_CallsRepositoryRemoveUserConnection(
        string userId, 
        string connectionId)
    {
        // Act
        await _connectionManager.OnUserDisconnectedAsync(userId, connectionId);

        // Assert
        _mockRepository.Verify(
            x => x.RemoveUserConnection(userId, connectionId),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnUserConnectedAsync_WhenMultipleConnections_CallsRepositoryForEachConnection(
        string userId)
    {
        // Arrange
        var connectionIds = new[] { "conn1", "conn2", "conn3" };

        // Act
        foreach (var connectionId in connectionIds)
        {
            await _connectionManager.OnUserConnectedAsync(userId, connectionId);
        }

        // Assert
        _mockRepository.Verify(
            x => x.AddUserConnection(userId, It.IsAny<string>()),
            Times.Exactly(connectionIds.Length));
    }

    [Theory, AutoData]
    public async Task OnUserConnectedAsync_WhenCalled_CompletesSuccessfully(
        string userId, 
        string connectionId)
    {
        // Act
        var task = _connectionManager.OnUserConnectedAsync(userId, connectionId);
        await task;

        // Assert
        Assert.True(task.IsCompletedSuccessfully);
    }

    [Theory, AutoData]
    public async Task OnUserDisconnectedAsync_WhenCalled_CompletesSuccessfully(
        string userId, 
        string connectionId)
    {
        // Act
        var task = _connectionManager.OnUserDisconnectedAsync(userId, connectionId);
        await task;

        // Assert
        Assert.True(task.IsCompletedSuccessfully);
    }
}