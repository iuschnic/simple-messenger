using Main.Application.Dtos;

namespace Main.API.Models;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}

public class SyncChatResponse
{
    public ChatSyncDto Chat { get; set; } = new();
}

public class SyncChatsResponse
{
    public List<ChatSyncDto> Chats { get; set; } = new();
}
