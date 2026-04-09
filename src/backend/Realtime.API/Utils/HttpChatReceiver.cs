namespace Realtime.API.Utils;

public class HttpChatReceiver(IConfiguration configuration)
{
    private readonly HttpClient _httpClient = new();
    private readonly string _mainServiceChatsUrl = configuration["MainServiceChatsUrl"] 
                                                   ?? throw new ArgumentNullException();
    
    public async Task<IEnumerable<Guid>> GetAllChatsByUserIdAsync(string userId)
    {
        var response = await _httpClient.GetAsync(_mainServiceChatsUrl + userId);
        
        response.EnsureSuccessStatusCode();
            
        var content = await response.Content.ReadFromJsonAsync<List<Guid>>();
        return content ?? [];
    }
}