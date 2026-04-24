using System.Collections.Concurrent;
using AutoFixture.Xunit2;
using Realtime.DB;

namespace Realtime.UnitTests;

public class ConnectionRepositoryTests
{
    private readonly ConnectionRepository _connectionRepository = new();

    [Theory, AutoData]
    public void AddUserConnection_WhenNewUser_AddsConnectionSuccessfully(
        string userId, 
        string connectionId)
    {
        // Act
        _connectionRepository.AddUserConnection(userId, connectionId);

        // Assert
        var connections = _connectionRepository.GetConnectionIdsByUserId(userId);
        Assert.Contains(connectionId, connections);
    }

    [Theory, AutoData]
    public void AddUserConnection_WhenSameUserMultipleConnections_AddsAllConnections(
        string userId, 
        string connectionId1, 
        string connectionId2)
    {
        // Act
        _connectionRepository.AddUserConnection(userId, connectionId1);
        _connectionRepository.AddUserConnection(userId, connectionId2);

        // Assert
        var connections = _connectionRepository.GetConnectionIdsByUserId(userId).ToList();
        Assert.Equal(2, connections.Count);
        Assert.Contains(connectionId1, connections);
        Assert.Contains(connectionId2, connections);
    }

    [Theory, AutoData]
    public void RemoveUserConnection_WhenConnectionExists_RemovesSuccessfully(
        string userId, 
        string connectionId)
    {
        // Arrange
        _connectionRepository.AddUserConnection(userId, connectionId);

        // Act
        _connectionRepository.RemoveUserConnection(userId, connectionId);

        // Assert
        var connections = _connectionRepository.GetConnectionIdsByUserId(userId);
        Assert.Empty(connections);
    }

    [Theory, AutoData]
    public void RemoveUserConnection_WhenUserDoesNotExist_DoesNotThrow(
        string userId, 
        string connectionId)
    {
        // Act & Assert
        var exception = Record.Exception(() =>
            _connectionRepository.RemoveUserConnection(userId, connectionId));

        Assert.Null(exception);
    }

    [Theory, AutoData]
    public void RemoveUserConnection_WhenLastConnectionRemoved_RemovesUserEntry(
        string userId, 
        string connectionId)
    {
        // Arrange
        _connectionRepository.AddUserConnection(userId, connectionId);

        // Act
        _connectionRepository.RemoveUserConnection(userId, connectionId);

        // Assert
        var connections = _connectionRepository.GetConnectionIdsByUserId(userId);
        Assert.Empty(connections);
    }

    [Theory, AutoData]
    public void GetConnectionIdsByUserId_WhenUserDoesNotExist_ReturnsEmptyEnumerable(
        string userId)
    {
        // Act
        var connections = _connectionRepository.GetConnectionIdsByUserId(userId);

        // Assert
        Assert.Empty(connections);
    }

    [Theory, AutoData]
    public void GetConnectionIdsByUserId_WhenUserHasConnections_ReturnsAllConnections(
        string userId, 
        string[] connectionIds)
    {
        // Arrange
        foreach (var connectionId in connectionIds)
        {
            _connectionRepository.AddUserConnection(userId, connectionId);
        }

        // Act
        var connections = _connectionRepository.GetConnectionIdsByUserId(userId).ToList();

        // Assert
        Assert.Equal(connectionIds.Length, connections.Count);
        foreach (var connectionId in connectionIds)
        {
            Assert.Contains(connectionId, connections);
        }
    }

    [Fact]
    public void Repository_WhenConcurrentOperations_PerformsThreadSafe()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        const int connectionCount = 100;
        var connections = new ConcurrentBag<string>();

        // Act
        Parallel.For(0, connectionCount, _ =>
        {
            var connectionId = Guid.NewGuid().ToString();
            connections.Add(connectionId);
            _connectionRepository.AddUserConnection(userId, connectionId);
        });

        // Assert
        var retrievedConnections = _connectionRepository.GetConnectionIdsByUserId(userId).ToList();
        Assert.Equal(connectionCount, retrievedConnections.Count);

        Parallel.ForEach(connections, connectionId =>
        {
            _connectionRepository.RemoveUserConnection(userId, connectionId);
        });

        Assert.Empty(_connectionRepository.GetConnectionIdsByUserId(userId));
    }
}