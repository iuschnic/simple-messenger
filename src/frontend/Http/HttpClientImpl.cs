using System.Net.Http.Headers;
using System.Net.Http.Json;
using BL.Contracts;
using BL.Models;
using Http.Dto;
using Http.Mapping;

namespace Http;

public class HttpClientImpl : IHttpClient
{
    private readonly HttpClient _http;
    private string? _token;

    public HttpClientImpl(HttpClient http, string baseUrl)
    {
        _http = http;
        _http.BaseAddress = new Uri(baseUrl);
    }

    // ================= AUTH =================

    public HttpResponseMessage Register(string uniqueName, string password, string email, string displayName)
    {
        var req = new
        {
            uniqueName,
            password,
            email,
            displayedName = displayName
        };

        return _http.PostAsJsonAsync("auth/register", req).Result;
    }
    public string Login(string uniqueName, string password)
    {
        var res = _http.PostAsJsonAsync("auth/login",
            new { uniqueName, password }).Result;

        res.EnsureSuccessStatusCode();

        var dto = res.Content.ReadFromJsonAsync<LoginResponseDto>().Result;

        _token = dto.Token;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        return _token!;
    }

    public User GetMe()
    {
        var res = _http.GetAsync("users/me").Result;
        res.EnsureSuccessStatusCode();

        return DtoMapper.ToUser(
            res.Content.ReadFromJsonAsync<UserDto>().Result
        );
    }


    // ================= USERS =================

    public User GetUser(Guid id)
    {
        var res = _http.GetAsync($"users/{id}").Result;
        res.EnsureSuccessStatusCode();

        return DtoMapper.ToUser(
            res.Content.ReadFromJsonAsync<UserDto>().Result
        );
    }

    public List<User> SearchUsers(string substr, int maxUsers)
    {
        var res = _http.GetAsync($"users?substr={substr}&maxUsers={maxUsers}").Result;
        res.EnsureSuccessStatusCode();

        return res.Content.ReadFromJsonAsync<List<UserDto>>().Result
            .Select(DtoMapper.ToUser)
            .ToList();
    }

    // ================= CHATS =================

    public List<Chat> GetChats()
    {
        var res = _http.GetAsync("chats").Result;
        res.EnsureSuccessStatusCode();

        return res.Content.ReadFromJsonAsync<List<ChatDto>>().Result
            .Select(DtoMapper.ToChat)
            .ToList();
    }

    public Chat CreateGroupChat(string name, List<Guid> memberIds)
    {
        var res = _http.PostAsJsonAsync("chats", new
        {
            chatType = "group",
            chatName = name,
            memberIds
        }).Result;

        res.EnsureSuccessStatusCode();

        // FIX: нормальный вариант — переспрос чата
        return GetChats().First(c => c.Name == name);
    }

    public Chat CreatePrivateChat(Guid withUserId)
    {
        var res = _http.PostAsJsonAsync("chats", new
        {
            chatType = "private",
            withUserId
        }).Result;

        res.EnsureSuccessStatusCode();

        return GetChats().First();
    }
    
    public List<SyncChatResult> SyncChats(List<(Guid chatId, long version)> chats)
    {
        var req = new
        {
            chats = chats.Select(c => new
            {
                chatId = c.chatId,
                clientVersion = c.version
            })
        };

        var res = _http.PostAsJsonAsync("chats/sync", req).Result;
        res.EnsureSuccessStatusCode();

        var dto = res.Content.ReadFromJsonAsync<SyncChatsResponseDto>().Result;

        return dto.SyncChats.Select(DtoMapper.ToSync).ToList();
    }

    
    public Chat GetChat(Guid chatId)
    {
        var res = _http.GetAsync($"chats/{chatId}").Result;

        if (res.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new Exception("Chat not found");

        res.EnsureSuccessStatusCode();

        var dto = res.Content.ReadFromJsonAsync<ChatDto>().Result;

        return DtoMapper.ToChat(dto);
    }
    
    // ================= MESSAGES =================

    public SyncChatResult SendMessage(Guid chatId, string text, long clientVersion)
    {
        var res = _http.PostAsJsonAsync(
            $"chats/{chatId}/messages",
            new
            {
                messageType = "send",
                clientVersion,
                text
            }).Result;

        res.EnsureSuccessStatusCode();

        return DtoMapper.ToSync(
            res.Content.ReadFromJsonAsync<SyncChatResponseDto>().Result
        );
    }

    public SyncChatResult EditMessage(Guid chatId, long messageNum, string newText, long clientVersion)
    {
        var res = _http.PatchAsJsonAsync(
            $"chats/{chatId}/messages/{messageNum}",
            new
            {
                newText,
                clientVersion
            }).Result;

        res.EnsureSuccessStatusCode();

        return DtoMapper.ToSync(
            res.Content.ReadFromJsonAsync<SyncChatResponseDto>().Result
        );
    }

    public SyncChatResult DeleteMessage(Guid chatId, long messageNum, long clientVersion)
    {
        var res = _http.DeleteAsync(
            $"chats/{chatId}/messages/{messageNum}?clientVersion={clientVersion}"
        ).Result;

        res.EnsureSuccessStatusCode();

        return DtoMapper.ToSync(
            res.Content.ReadFromJsonAsync<SyncChatResponseDto>().Result
        );
    }

    public List<Message> GetMessages(Guid chatId, long? fromMessageNumber = null, int? limit = null)
    {
        var url = $"chats/{chatId}/messages?";

        if (fromMessageNumber != null)
            url += $"fromMessageNumber={fromMessageNumber}&";

        if (limit != null)
            url += $"limit={limit}";

        var res = _http.GetAsync(url).Result;
        res.EnsureSuccessStatusCode();

        return res.Content.ReadFromJsonAsync<List<MessageDto>>().Result
            .Select(DtoMapper.ToMessage)
            .ToList();
    }
    
    public SyncChatResult RemoveUserFromChat(Guid chatId, Guid userId)
    {
        var res = _http.DeleteAsync(
            $"chats/{chatId}/members/{userId}"
        ).Result;

        res.EnsureSuccessStatusCode();

        return DtoMapper.ToSync(
            res.Content.ReadFromJsonAsync<SyncChatResponseDto>().Result
        );
    }
    
    public List<Message> GetMessages(Guid chatId, long? fromMessageNumber = null, int? limit = null, long? clientVersion = null)
    {
        var url = $"chats/{chatId}/messages?";

        if (fromMessageNumber != null)
            url += $"fromMessageNumber={fromMessageNumber}&";

        if (limit != null)
            url += $"limit={limit}&";

        if (clientVersion != null)
            url += $"clientVersion={clientVersion}";

        var res = _http.GetAsync(url).Result;
        res.EnsureSuccessStatusCode();

        return res.Content.ReadFromJsonAsync<List<MessageDto>>().Result
            .Select(DtoMapper.ToMessage)
            .ToList();
    }
    
    //добавила
    public User GetUserByName(string uniqueName)
    {
        var res = _http.GetAsync($"users/{uniqueName}").Result;
        res.EnsureSuccessStatusCode();

        return DtoMapper.ToUser(
            res.Content.ReadFromJsonAsync<UserDto>().Result
        );
    }
    
    public CurrentUser UpdateMeDisplayName(string displayName)
    {
        var res = _http.PatchAsJsonAsync(
            "users/me",
            new
            {
                displayName
            }
        ).Result;

        res.EnsureSuccessStatusCode();

        return DtoMapper.ToCurrentUser(
            res.Content.ReadFromJsonAsync<CurrentUserDto>().Result
        );
    }
    
    //поправть хз как на сервере
    public User UpdateContactName(Guid id, string contactName)
    {
        var res = _http.PatchAsJsonAsync(
            "users/{id}",
            new
            {
                id
            }
        ).Result;

        res.EnsureSuccessStatusCode();

        return DtoMapper.ToUser(
            res.Content.ReadFromJsonAsync<UserDto>().Result
        );
    }
}

