using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BL.Contracts;
using BL.Models;
using Http.Dto;
using Http.Mapping;
using BL.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Http;

public class HttpClientImpl : IHttpClient
{
    private readonly HttpClient _http;
    private string? _token;

    public HttpClientImpl(HttpClient http, IConfiguration config)
    {
        _http = http;

        var baseUrl = config["Backend:ApiBaseUrl"]
                      ?? throw new Exception("ApiBaseUrl not configured");

        _http.BaseAddress = new Uri(baseUrl);
    }

    // ================= SAFE SEND =================

    private async Task<HttpResponseMessage> Send(Func<Task<HttpResponseMessage>> action)
    {
        try
        {
            return await action();
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

    private async Task HandleErrors(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode)
            return;

        var raw = await res.Content.ReadAsStringAsync();
        string message = raw;

        try
        {
            using var json = JsonDocument.Parse(raw);
            if (json.RootElement.TryGetProperty("message", out var msg))
                message = msg.GetString() ?? raw;
        }
        catch
        {
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

    private async Task<T> Read<T>(HttpResponseMessage res)
    {
        try
        {
            var result = await res.Content.ReadFromJsonAsync<T>();

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

    public async Task Register(string uniqueName, string password, string email, string displayName)
    {
        var res = await Send(() => _http.PostAsJsonAsync("auth/register", new
        {
            uniqueName,
            password,
            email,
            displayedName = displayName
        }));

        await HandleErrors(res);
    }

    public async Task<string> Login(string uniqueName, string password)
    {
        var res = await Send(() => _http.PostAsJsonAsync("auth/login",
            new { uniqueName, password }));

        await HandleErrors(res);

        var dto = await Read<LoginResponseDto>(res);

        _token = dto.Token;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        return _token!;
    }

    public async Task<User> GetMe()
    {
        var res = await Send(() => _http.GetAsync("users/me"));

        await HandleErrors(res);

        return DtoMapper.ToUser(await Read<UserDto>(res));
    }

    // ================= USERS =================

    public async Task<User> GetUser(Guid id)
    {
        var res = await Send(() => _http.GetAsync($"users/{id}"));

        await HandleErrors(res);

        return DtoMapper.ToUser(await Read<UserDto>(res));
    }

    public async Task<User> GetUserByName(string uniqueName)
    {
        var users = await SearchUsers(uniqueName, 1);

        if (!users.Any())
            throw new NotFoundException("User not found");

        return users.First();
    }

    public async Task<List<User>> SearchUsers(string substr, int maxUsers)
    {
        var res = await Send(() => _http.GetAsync($"users?substr={substr}&maxUsers={maxUsers}"));

        await HandleErrors(res);

        var list = await Read<List<UserDto>>(res);

        return list.Select(DtoMapper.ToUser).ToList();
    }

    public async Task<CurrentUser> UpdateMeDisplayName(string displayName)
    {
        var res = await Send(() => _http.PatchAsJsonAsync("users/me",
            new { newDisplayedName = displayName }));

        await HandleErrors(res);

        return DtoMapper.ToCurrentUser(await Read<CurrentUserDto>(res));
    }

    public async Task<User> UpdateContactName(Guid id, string contactName)
    {
        var res = await Send(() => _http.PatchAsJsonAsync(
            $"users/me/contacts/{id}",
            new { newContactName = contactName }
        ));

        await HandleErrors(res);

        return DtoMapper.ToUser(await Read<UserDto>(res));
    }

    // ================= CHATS =================

    public async Task<List<Chat>> GetChats()
    {
        var res = await Send(() => _http.GetAsync("chats"));

        await HandleErrors(res);

        var list = await Read<List<ChatDto>>(res);

        return list.Select(DtoMapper.ToChat).ToList();
    }

    public async Task<Chat> CreateGroupChat(string name, List<Guid> memberIds)
    {
        var res = await Send(() => _http.PostAsJsonAsync("chats", new
        {
            chatType = 0,
            chatName = name,
            memberIds
        }));

        await HandleErrors(res);

        await Read<SyncChatResponseDto>(res);

        return (await GetChats()).First(c => c.Name == name);
    }

    public async Task<Chat> CreatePrivateChat(Guid withUserId)
    {
        var res = await Send(() => _http.PostAsJsonAsync("chats", new
        {
            chatType = 1,
            withUserId
        }));

        await HandleErrors(res);

        await Read<SyncChatResponseDto>(res);

        return (await GetChats()).First();
    }

    public async Task<Chat> GetChat(Guid chatId)
    {
        var res = await Send(() => _http.GetAsync($"chats/{chatId}"));

        await HandleErrors(res);

        return DtoMapper.ToChat(await Read<ChatDto>(res));
    }
    
    public async Task<SyncChatResult> SyncChat(Guid chatId, ulong clientVersion)
    {
        var res = await Send(() => _http.PostAsJsonAsync(
            $"chats/sync/{chatId}",
            new
            {
                clientVersion
            }));

        await HandleErrors(res);

        return DtoMapper.ToSync(await Read<SyncChatResponseDto>(res));
    }

    public async Task<List<SyncChatResult>> SyncChats(List<(Guid chatId, ulong version)> chats)
    {
        var res = await Send(() => _http.PostAsJsonAsync("chats/sync", new
        {
            chats = chats.Select(c => new
            {
                chatId = c.chatId,
                clientVersion = c.version
            })
        }));

        await HandleErrors(res);

        var dto = await Read<SyncChatsResponseDto>(res);

        return dto.Chats.Select(DtoMapper.ToSync).ToList();
    }

    public async Task<SyncChatResult> RemoveUserFromChat(Guid chatId, Guid userId, ulong clientVersion)
    {
        var res = await Send(() => _http.DeleteAsync(
            $"chats/{chatId}/members/{userId}?clientVersion={clientVersion}"
        ));

        await HandleErrors(res);

        return DtoMapper.ToSync(await Read<SyncChatResponseDto>(res));
    }

    // ================= MESSAGES =================

    public async Task<SyncChatResult> SendMessage(Guid chatId, string text, ulong clientVersion)
    {
        var res = await Send(() => _http.PostAsJsonAsync(
            $"chats/{chatId}/messages",
            new
            {
                messageType = 0,
                clientVersion,
                text
            }));

        await HandleErrors(res);

        return DtoMapper.ToSync(await Read<SyncChatResponseDto>(res));
    }

    public async Task<SyncChatResult> EditMessage(Guid chatId, ulong messageNum, string newText, ulong clientVersion)
    {
        var res = await Send(() => _http.PatchAsJsonAsync(
            $"chats/{chatId}/messages/{messageNum}",
            new { newText, clientVersion }));

        await HandleErrors(res);

        return DtoMapper.ToSync(await Read<SyncChatResponseDto>(res));
    }

    public async Task<SyncChatResult> DeleteMessage(Guid chatId, ulong messageNum, ulong clientVersion)
    {
        var res = await Send(() => _http.DeleteAsync(
            $"chats/{chatId}/messages/{messageNum}?clientVersion={clientVersion}"
        ));

        await HandleErrors(res);

        return DtoMapper.ToSync(await Read<SyncChatResponseDto>(res));
    }

    public async Task<List<Message>> GetMessages(Guid chatId, ulong? fromMessageNumber = null, int? limit = null)
    {
        var url = $"chats/{chatId}/messages?";

        if (fromMessageNumber != null)
            url += $"fromMessageNum={fromMessageNumber}&";

        if (limit != null)
            url += $"limit={limit}";

        var res = await Send(() => _http.GetAsync(url));

        await HandleErrors(res);

        var list = await Read<List<MessageDto>>(res);

        return list.Select(DtoMapper.ToMessage).ToList();
    }
}