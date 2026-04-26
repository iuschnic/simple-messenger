using System.Security.Claims;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Realtime.API.Utils;
using Realtime.BL.InputPorts;
using Realtime.BL.OutputPorts;
using Serilog;
using IGroupManager = Realtime.BL.InputPorts.IGroupManager;

namespace Realtime.UnitTests;

public class RealtimeHubTests
{
    private readonly Mock<IConnectionManager> _mockConnectionManager;
    private readonly Mock<IGroupManager> _mockGroupManager;
    private readonly Mock<IHttpChatReceiver> _mockChatReceiver;
    private readonly Mock<ILogger> _mockLogger;
    private readonly RealtimeHub _realtimeHub;

    public RealtimeHubTests()
    {
        _mockConnectionManager = new Mock<IConnectionManager>();
        _mockGroupManager = new Mock<IGroupManager>();
        _mockChatReceiver = new Mock<IHttpChatReceiver>();
        _mockLogger = new Mock<ILogger>();

        _realtimeHub = new RealtimeHub(
            _mockConnectionManager.Object,
            _mockGroupManager.Object,
            _mockChatReceiver.Object,
            _mockLogger.Object);
    }

    private static void SetupHubContext(RealtimeHub hub, string userId, string connectionId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var context = new Mock<HubCallerContext>();
        context.Setup(c => c.User).Returns(principal);
        context.Setup(c => c.ConnectionId).Returns(connectionId);

        hub.Context = context.Object;
    }

    [Theory, AutoData]
    public async Task OnConnectedAsync_WhenAuthorized_AddsUserToGroups(
        string userId, 
        string connectionId)
    {
        // Arrange
        SetupHubContext(_realtimeHub, userId, connectionId);

        var chatIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        _mockChatReceiver
            .Setup(x => x.GetAllChatsByUserIdAsync(userId))
            .ReturnsAsync(chatIds);

        // Act
        await _realtimeHub.OnConnectedAsync();

        // Assert
        _mockConnectionManager.Verify(
            x => x.OnUserConnectedAsync(userId, connectionId),
            Times.Once);

        foreach (var chatId in chatIds)
        {
            _mockGroupManager.Verify(
                x => x.AddToGroupAsync(connectionId, chatId.ToString()),
                Times.Once);
        }

        _mockLogger.Verify(
            x => x.Information("User {UserId} connected", userId),
            Times.Once);
    }

    [Fact]
    public async Task OnConnectedAsync_WhenUnauthorized_ThrowsHubException()
    {
        // Arrange
        var context = new Mock<HubCallerContext>();
        context.Setup(c => c.User).Returns((ClaimsPrincipal)null!);
        _realtimeHub.Context = context.Object;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HubException>(
            () => _realtimeHub.OnConnectedAsync());

        Assert.Equal("Unauthorized: Please provide valid JWT token", exception.Message);

        _mockLogger.Verify(
            x => x.Error("Attempted unauthorized connection"),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnDisconnectedAsync_WhenAuthorized_RemovesUserFromGroups(
        string userId, 
        string connectionId)
    {
        // Arrange
        SetupHubContext(_realtimeHub, userId, connectionId);

        var chatIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        _mockChatReceiver
            .Setup(x => x.GetAllChatsByUserIdAsync(userId))
            .ReturnsAsync(chatIds);

        // Act
        await _realtimeHub.OnDisconnectedAsync(null);

        // Assert
        _mockConnectionManager.Verify(
            x => x.OnUserDisconnectedAsync(userId, connectionId),
            Times.Once);

        foreach (var chatId in chatIds)
        {
            _mockGroupManager.Verify(
                x => x.RemoveFromGroupAsync(connectionId, chatId.ToString()),
                Times.Once);
        }

        _mockLogger.Verify(
            x => x.Information("User {UserId} disconnected", userId),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WhenUnauthorized_ThrowsHubException()
    {
        // Arrange
        var context = new Mock<HubCallerContext>();
        context.Setup(c => c.User).Returns((ClaimsPrincipal)null!);
        _realtimeHub.Context = context.Object;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HubException>(
            () => _realtimeHub.OnDisconnectedAsync(null));

        Assert.Equal("Unauthorized: Please provide valid JWT token", exception.Message);

        _mockLogger.Verify(
            x => x.Error("Attempted unauthorized disconnection"),
            Times.Once);
    }

    [Theory, AutoData]
    public async Task OnDisconnectedAsync_WhenExceptionProvided_HandlesGracefully(
        string userId, 
        string connectionId)
    {
        // Arrange
        SetupHubContext(_realtimeHub, userId, connectionId);
        var testException = new Exception("Test disconnection exception");

        // Act
        await _realtimeHub.OnDisconnectedAsync(testException);

        // Assert
        _mockConnectionManager.Verify(
            x => x.OnUserDisconnectedAsync(userId, connectionId),
            Times.Once);
    }
}