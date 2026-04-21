using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BL.Contracts;
using BL.Models;
using Http.Dto;
using Http.Mapping;
using BL.Exceptions;

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

    // ================= SAFE SEND =================

    private HttpResponseMessage Send(Func<HttpResponseMessage> action)
    {
        try
        {
            return action();
        }
        catch (HttpRequestException ex)
        {
            throw new ApiException(0, $"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            throw new ApiException(0, "Request timeout");
        }
    }

    // ================= ERROR HANDLER =================

    private void HandleErrors(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode)
            return;

        var raw = res.Content.ReadAsStringAsync().Result;
        string message = raw;

        try
        {
            using var json = JsonDocument.Parse(raw);
            if (json.RootElement.TryGetProperty("message", out var msg))
                message = msg.GetString() ?? raw;
        }
        catch
        {
            // ignore, leave raw
        }

        switch (res.StatusCode)
        {
            case HttpStatusCode.BadRequest:
                throw new BadRequestException(message);

            case HttpStatusCode.Unauthorized:
                throw new UnauthorizedException();

            case HttpStatusCode.Forbidden:
                throw new ForbiddenException(message);

            case HttpStatusCode.NotFound:
                throw new NotFoundException(message);

            case HttpStatusCode.Conflict:
                throw new ConflictException(message);

            default:
                if ((int)res.StatusCode >= 500)
                    throw new ServerException((int)res.StatusCode, message);

                throw new ApiException((int)res.StatusCode, message);
        }
    }

    private T Read<T>(HttpResponseMessage res)
    {
        try
        {
            var result = res.Content.ReadFromJsonAsync<T>().Result;

            if (result == null)
                throw new ApiException(0, "Empty response from server");

            return result;
        }
        catch (Exception ex)
        {
            throw new ApiException(0, $"Invalid response format: {ex.Message}");
        }
    }

    // ================= AUTH =================

    public void Register(string uniqueName, string password, string email, string displayName)
    {
        var res = Send(() => _http.PostAsJsonAsync("auth/register", new
        {
            uniqueName,
            password,
            email,
            displayedName = displayName
        }).Result);

        HandleErrors(res);
    }

    public string Login(string uniqueName, string password)
    {
        var res = Send(() => _http.PostAsJsonAsync("auth/login",
            new { uniqueName, password }).Result);

        HandleErrors(res);

        var dto = Read<LoginResponseDto>(res);

        _token = dto.Token;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        return _token!;
    }

    public User GetMe()
    {
        var res = Send(() => _http.GetAsync("users/me").Result);

        HandleErrors(res);

        return DtoMapper.ToUser(Read<UserDto>(res));
    }

    // ================= USERS =================

    public User GetUser(Guid id)
    {
        var res = Send(() => _http.GetAsync($"users/{id}").Result);

        HandleErrors(res);

        return DtoMapper.ToUser(Read<UserDto>(res));
    }

    public User GetUserByName(string uniqueName)
    {
        var users = SearchUsers(uniqueName, 1);

        if (!users.Any())
            throw new NotFoundException("User not found");

        return users.First();
    }

    public List<User> SearchUsers(string substr, int maxUsers)
    {
        var res = Send(() => _http.GetAsync($"users?substr={substr}&maxUsers={maxUsers}").Result);

        HandleErrors(res);

        return Read<List<UserDto>>(res)
            .Select(DtoMapper.ToUser)
            .ToList();
    }

    public CurrentUser UpdateMeDisplayName(string displayName)
    {
        var res = Send(() => _http.PatchAsJsonAsync("users/me",
            new { newDisplayedName = displayName }).Result);

        HandleErrors(res);

        return DtoMapper.ToCurrentUser(Read<CurrentUserDto>(res));
    }

    public User UpdateContactName(Guid id, string contactName)
    {
        var res = Send(() => _http.PatchAsJsonAsync(
            $"users/me/contacts/{id}",
            new { newContactName = contactName }
        ).Result);

        HandleErrors(res);

        return DtoMapper.ToUser(Read<UserDto>(res));
    }

    // ================= CHATS =================

    public List<Chat> GetChats()
    {
        var res = Send(() => _http.GetAsync("chats").Result);

        HandleErrors(res);

        return Read<List<ChatDto>>(res)
            .Select(DtoMapper.ToChat)
            .ToList();
    }

    public Chat CreateGroupChat(string name, List<Guid> memberIds)
    {
        var res = Send(() => _http.PostAsJsonAsync("chats", new
        {
            chatType = 0,
            chatName = name,
            memberIds
        }).Result);

        HandleErrors(res);

        var dto = Read<SyncChatResponseDto>(res);

        return GetChats().First(c => c.Name == name);
    }

    public Chat CreatePrivateChat(Guid withUserId)
    {
        var res = Send(() => _http.PostAsJsonAsync("chats", new
        {
            chatType = 1,
            withUserId
        }).Result);

        HandleErrors(res);

        var dto = Read<SyncChatResponseDto>(res);

        return GetChats().First();
    }

    public Chat GetChat(Guid chatId)
    {
        var res = Send(() => _http.GetAsync($"chats/{chatId}").Result);

        HandleErrors(res);

        return DtoMapper.ToChat(Read<ChatDto>(res));
    }

    public List<SyncChatResult> SyncChats(List<(Guid chatId, ulong version)> chats)
    {
        var res = Send(() => _http.PostAsJsonAsync("chats/sync", new
        {
            chats = chats.Select(c => new
            {
                chatId = c.chatId,
                clientVersion = c.version
            })
        }).Result);

        HandleErrors(res);

        var dto = Read<SyncChatsResponseDto>(res);

        return dto.SyncChats.Select(DtoMapper.ToSync).ToList();
    }

    public SyncChatResult RemoveUserFromChat(Guid chatId, Guid userId)
    {
        var res = Send(() => _http.DeleteAsync($"chats/{chatId}/members/{userId}").Result);

        HandleErrors(res);

        return DtoMapper.ToSync(Read<SyncChatResponseDto>(res));
    }

    // ================= MESSAGES =================

    public SyncChatResult SendMessage(Guid chatId, string text, ulong clientVersion)
    {
        var res = Send(() => _http.PostAsJsonAsync(
            $"chats/{chatId}/messages",
            new
            {
                messageType = 0,
                clientVersion,
                text
            }).Result);

        HandleErrors(res);

        return DtoMapper.ToSync(Read<SyncChatResponseDto>(res));
    }

    public SyncChatResult EditMessage(Guid chatId, ulong messageNum, string newText, ulong clientVersion)
    {
        var res = Send(() => _http.PatchAsJsonAsync(
            $"chats/{chatId}/messages/{messageNum}",
            new { newText, clientVersion }).Result);

        HandleErrors(res);

        return DtoMapper.ToSync(Read<SyncChatResponseDto>(res));
    }

    public SyncChatResult DeleteMessage(Guid chatId, ulong messageNum, ulong clientVersion)
    {
        var res = Send(() => _http.DeleteAsync(
            $"chats/{chatId}/messages/{messageNum}?clientVersion={clientVersion}"
        ).Result);

        HandleErrors(res);

        return DtoMapper.ToSync(Read<SyncChatResponseDto>(res));
    }

    public List<Message> GetMessages(Guid chatId, ulong? fromMessageNumber = null, int? limit = null)
    {
        var url = $"chats/{chatId}/messages?";

        if (fromMessageNumber != null)
            url += $"fromMessageNum={fromMessageNumber}&";

        if (limit != null)
            url += $"limit={limit}";

        var res = Send(() => _http.GetAsync(url).Result);

        HandleErrors(res);

        return Read<List<MessageDto>>(res)
            .Select(DtoMapper.ToMessage)
            .ToList();
    }
}