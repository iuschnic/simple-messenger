using Realtime.BL.InputPorts;

namespace Realtime.API.Utils;

public class HttpChatReceiver : IHttpChatReceiver
{
    private readonly HttpClient _httpClient;
    private readonly string _mainServiceChatsUrl;
    private readonly string _apiKey;

    public HttpChatReceiver(IConfiguration configuration)
    {
        _httpClient = new HttpClient();
        _mainServiceChatsUrl = configuration["MainServiceChatsUrl"]
            ?? throw new ArgumentNullException("MainServiceChatsUrl is not configured");

        _apiKey = configuration["InternalApiKey"]
            ?? throw new ArgumentNullException("InternalApiKey is not configured");
    }

    public async Task<IEnumerable<Guid>> GetAllChatsByUserIdAsync(string userId)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_mainServiceChatsUrl}/{userId}");

        request.Headers.Add("X-Api-Key", _apiKey);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadFromJsonAsync<List<Guid>>();
        return content ?? [];
    }
}