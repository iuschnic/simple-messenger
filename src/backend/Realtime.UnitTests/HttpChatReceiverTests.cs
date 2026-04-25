using System.Net;
using System.Text.Json;
using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Realtime.API.Utils;

namespace Realtime.UnitTests;

public class HttpChatReceiverTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpChatReceiver _httpChatReceiver;
    private readonly string _baseUrl;

    public HttpChatReceiverTests()
    {
        _baseUrl = "http://test-api/chats/";
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration
            .Setup(x => x["MainServiceChatsUrl"])
            .Returns(_baseUrl);
        mockConfiguration
            .Setup(x => x["InternalApiKey"])
            .Returns(string.Empty);

        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        
        _httpChatReceiver = new TestableHttpChatReceiver(mockConfiguration.Object, httpClient);
    }

    private class TestableHttpChatReceiver : HttpChatReceiver
    {
        public TestableHttpChatReceiver(IConfiguration configuration, HttpClient httpClient)
            : base(configuration)
        {
            var field = typeof(HttpChatReceiver).GetField("_httpClient",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            field?.SetValue(this, httpClient);
        }
    }

    [Theory, AutoData]
    public async Task GetAllChatsByUserIdAsync_WhenSuccessful_ReturnsChats(
        string userId)
    {
        // Arrange
        var expectedChats = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var responseJson = JsonSerializer.Serialize(expectedChats);

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == _baseUrl + userId),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseJson)
            });

        // Act
        var result = await _httpChatReceiver.GetAllChatsByUserIdAsync(userId);

        // Assert
        Assert.Equal(expectedChats, result);
    }

    [Theory, AutoData]
    public async Task GetAllChatsByUserIdAsync_WhenResponseIsEmpty_ReturnsEmptyList(
        string userId)
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("[]")
            });

        // Act
        var result = await _httpChatReceiver.GetAllChatsByUserIdAsync(userId);

        // Assert
        Assert.Empty(result);
    }

    [Theory, AutoData]
    public async Task GetAllChatsByUserIdAsync_WhenResponseIsNotSuccessful_ThrowsHttpRequestException(
        string userId)
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(
            () => _httpChatReceiver.GetAllChatsByUserIdAsync(userId));
    }

    [Fact]
    public void Constructor_WhenMainServiceChatsUrlNotConfigured_ThrowsArgumentNullException()
    {
        // Arrange
        var configuration = new Mock<IConfiguration>();
        configuration
            .Setup(x => x["MainServiceChatsUrl"])
            .Returns((string)null!);
        configuration
            .Setup(x => x["InternalApiKey"])
            .Returns(string.Empty);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new HttpChatReceiver(configuration.Object));
    }
}