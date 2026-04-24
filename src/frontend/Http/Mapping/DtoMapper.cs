using BL.Models;
using Http.Dto;

namespace Http.Mapping;

public static class DtoMapper
{
    // ================= USERS =================

    public static User ToUser(UserDto d) => new()
    {
        Id = d.Id,
        UniqueName = d.UniqueName,
        DisplayName = d.DisplayedName
    };
    

    public static User ToUser(ContactDto dto) => new()
    {
        Id = dto.ContactUser.Id,
        UniqueName = dto.ContactUser.UniqueName,
        DisplayName = dto.ContactUser.DisplayedName,
        ContactName = dto.ContactName
    };
    public static CurrentUser ToCurrentUser(UserDto d) => new()
    {
        UniqueName = d.UniqueName,
        DisplayedName = d.DisplayedName,

        // этих данных нет в API
        Email = null!,
        PasswordHash = null!
    };
    // ================= CHATS =================

    public static Chat ToChat(ChatDto d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        OwnerId = d.OwnerUserId,
        CreatedAt = d.CreatedAt,
        Version = d.Version,
        Type = (ChatType)d.Type,
        LastMessageNum = d.LastMessageNum,
        Members = d.Participants?
            .Select(ToUser)
            .ToList() ?? new List<User>()
    };

    // ================= MESSAGES =================

    public static Message ToMessage(MessageDto d) => new()
    {
        MessageNumber = d.MessageNum,
        ChatId = d.ChatId,
        SenderId = d.SenderId,
        Text = d.Text,
        CreatedAt = d.CreatedAt,
        EditedAt = d.EditedAt,
        Deleted = d.Deleted,
        Version = d.Version,
        Type = (MessageType)d.Type,
    };

    // ================= SYNC =================

    public static SyncChatResult ToSync(ChatSyncDto chat)
    {
        return new SyncChatResult
        {
            ChatId = chat.ChatId,

            LastVersion = chat.ChatMeta?.Version ?? 0,

            ChatName = chat.ChatMeta?.Name,
            ChatType = chat.ChatMeta != null
                ? (ChatType)chat.ChatMeta.Type
                : default,
            LastMessageNum = chat.ChatMeta?.LastMessageNum ?? 0,

            Messages = chat.Messages?
                .Select(ToMessage)
                .ToList() ?? new List<Message>(),

            Participants = chat.Participants?
                .Select(p => new User
                {
                    Id = p.UserId,
                    UniqueName = p.UniqueName,
                    DisplayName = p.DisplayedName,
                })
                .ToList() ?? new List<User>()
        };
    }
}