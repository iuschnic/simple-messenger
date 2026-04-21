using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    // ================= ERROR HANDLER =================

    private void HandleErrors(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode)
            return;

        var message = res.Content.ReadAsStringAsync().Result;

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

    // ================= AUTH =================

    public void Register(string uniqueName, string password, string email, string displayName)
    {
        var res = _http.PostAsJsonAsync("auth/register", new
        {
            uniqueName,
            password,
            email,
            displayedName = displayName
        }).Result;

        HandleErrors(res);
    }

    public string Login(string uniqueName, string password)
    {
        var res = _http.PostAsJsonAsync("auth/login",
            new { uniqueName, password }).Result;

        HandleErrors(res);

        var dto = res.Content.ReadFromJsonAsync<LoginResponseDto>().Result;

        _token = dto.Token;

        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _token);

        return _token!;
    }

    public User GetMe()
    {
        var res = _http.GetAsync("users/me").Result;

        HandleErrors(res);

        return DtoMapper.ToUser(
            res.Content.ReadFromJsonAsync<UserDto>().Result
        );
    }

    // ================= USERS =================

    public User GetUser(Guid id)
    {
        var res = _http.GetAsync($"users/{id}").Result;

        HandleErrors(res);

        return DtoMapper.ToUser(
            res.Content.ReadFromJsonAsync<UserDto>().Result
        );
    }

    public User GetUserByName(string uniqueName)
    {
        var res = _http.GetAsync($"users/{uniqueName}").Result;

        HandleErrors(res);

        return DtoMapper.ToUser(
            res.Content.ReadFromJsonAsync<UserDto>().Result
        );
    }

    public List<User> SearchUsers(string substr, int maxUsers)
    {
        var res = _http.GetAsync($"users?substr={substr}&maxUsers={maxUsers}").Result;

        HandleErrors(res);

        return res.Content.ReadFromJsonAsync<List<UserDto>>().Result!
            .Select(DtoMapper.ToUser)
            .ToList();
    }

    public CurrentUser UpdateMeDisplayName(string displayName)
    {
        var res = _http.PatchAsJsonAsync("users/me",
            new { newDisplayedName = displayName }).Result;

        HandleErrors(res);

        return DtoMapper.ToCurrentUser(
            res.Content.ReadFromJsonAsync<CurrentUserDto>().Result
        );
    }

    public User UpdateContactName(Guid id, string contactName)
    {
        var res = _http.PatchAsJsonAsync(
            $"users/me/contacts/{id}",
            new { newContactName = contactName }
        ).Result;

        HandleErrors(res);
        
        return DtoMapper.ToUser(
            res.Content.ReadFromJsonAsync<UserDto>().Result
        );
    }

    // ================= CHATS =================

    public List<Chat> GetChats()
    {
        var res = _http.GetAsync("chats").Result;

        HandleErrors(res);

        return res.Content.ReadFromJsonAsync<List<ChatDto>>().Result!
            .Select(DtoMapper.ToChat)
            .ToList();
    }

    public Chat CreateGroupChat(string name, List<Guid> memberIds)
    {
        var res = _http.PostAsJsonAsync("chats", new
        {
            chatType = 0,
            chatName = name,
            memberIds
        }).Result;

        HandleErrors(res);
        
        return GetChats().First(c => c.Name == name);
    }

    public Chat CreatePrivateChat(Guid withUserId)
    {
        var res = _http.PostAsJsonAsync("chats", new
        {
            chatType = 1,
            withUserId
        }).Result;

        HandleErrors(res);

        return GetChats().First();
    }

    public Chat GetChat(Guid chatId)
    {
        var res = _http.GetAsync($"chats/{chatId}").Result;

        HandleErrors(res);

        var dto = res.Content.ReadFromJsonAsync<ChatDto>().Result;

        return DtoMapper.ToChat(dto);
    }

    public List<SyncChatResult> SyncChats(List<(Guid chatId, ulong version)> chats)
    {
        var res = _http.PostAsJsonAsync("chats/sync", new
        {
            chats = chats.Select(c => new
            {
                chatId = c.chatId,
                clientVersion = c.version
            })
        }).Result;

        HandleErrors(res);

        var dto = res.Content.ReadFromJsonAsync<SyncChatsResponseDto>().Result;

        return dto.SyncChats.Select(DtoMapper.ToSync).ToList();
    }

    public SyncChatResult RemoveUserFromChat(Guid chatId, Guid userId)
    {
        var res = _http.DeleteAsync($"chats/{chatId}/members/{userId}").Result;

        HandleErrors(res);

        return DtoMapper.ToSync(
            res.Content.ReadFromJsonAsync<SyncChatResponseDto>().Result
        );
    }

    // ================= MESSAGES =================

    public SyncChatResult SendMessage(Guid chatId, string text, ulong clientVersion)
    {
        var res = _http.PostAsJsonAsync(
            $"chats/{chatId}/messages",
            new
            {
                messageType = 0,
                clientVersion,
                text
            }).Result;

        HandleErrors(res);

        return DtoMapper.ToSync(
            res.Content.ReadFromJsonAsync<SyncChatResponseDto>().Result
        );
    }

    public SyncChatResult EditMessage(Guid chatId, ulong messageNum, string newText, ulong clientVersion)
    {
        var res = _http.PatchAsJsonAsync(
            $"chats/{chatId}/messages/{messageNum}",
            new { newText, clientVersion }).Result;

        HandleErrors(res);

        return DtoMapper.ToSync(
            res.Content.ReadFromJsonAsync<SyncChatResponseDto>().Result
        );
    }

    public SyncChatResult DeleteMessage(Guid chatId, ulong messageNum, ulong clientVersion)
    {
        var res = _http.DeleteAsync(
            $"chats/{chatId}/messages/{messageNum}?clientVersion={clientVersion}"
        ).Result;

        HandleErrors(res);

        return DtoMapper.ToSync(
            res.Content.ReadFromJsonAsync<SyncChatResponseDto>().Result
        );
    }

    public List<Message> GetMessages(Guid chatId, ulong? fromMessageNumber = null, int? limit = null)
    {
        var url = $"chats/{chatId}/messages?";

        if (fromMessageNumber != null)
            url += $"fromMessageNum={fromMessageNumber}&";

        if (limit != null)
            url += $"limit={limit}";

        var res = _http.GetAsync(url).Result;

        HandleErrors(res);

        return res.Content.ReadFromJsonAsync<List<MessageDto>>().Result!
            .Select(DtoMapper.ToMessage)
            .ToList();
    }
}