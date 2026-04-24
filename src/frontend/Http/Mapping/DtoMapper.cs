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
        DisplayName = d.DisplayName
    };

    public static CurrentUser ToCurrentUser(CurrentUserDto d) => new()
    {
        UniqueName = d.UniqueName,
        PasswordHash = d.PasswordHash,
        DisplayedName = d.DisplayedName,
        Email = d.Email,
    };

    // ================= CHATS =================

    public static Chat ToChat(ChatDto d) => new()
    {
        Id = d.ChatId,
        Name = d.Name,
        OwnerId = d.OwnerId,
        CreatedAt = d.CreatedAt,
        Version = d.Version,
        Type = (ChatType)d.Type,
        LastMessageNum = d.LastMessageNum,
        Members = d.Members?
            .Select(ToUser)
            .ToList() ?? new List<User>()
    };

    // ================= MESSAGES =================

    public static Message ToMessage(MessageDto d) => new()
    {
        MessageNumber = d.MessageNumber,
        ChatId = d.ChatId,
        SenderId = d.SenderId,
        Text = d.Text,
        CreatedAt = d.CreatedAt,
        EditedAt = d.EditedAt,
        Deleted = d.Deleted,
        Version = d.Version,
        Type = (MessageType)d.Type
    };
    
    public static User ToUser(ContactDto dto)
    {
        return new User
        {
            Id = dto.ContactUser.Id,
            UniqueName = dto.ContactUser.UniqueName,
            DisplayName = dto.ContactUser.DisplayName,
            ContactName = dto.ContactName
        };
    }

    // ================= SYNC =================

    public static SyncChatResult ToSync(SyncChatResponseDto d)
    {
        var chat = d.Chat;

        return new SyncChatResult
        {
            ChatId = chat.ChatId,

            // версия берётся из meta
            LastVersion = chat.ChatMeta?.Version ?? 0,

            // сообщения
            Messages = chat.Messages?
                .Select(ToMessage)
                .ToList() ?? new List<Message>(),

            // участники
            Participants = chat.Participants?
                .Select(p => new User
                {
                    Id = p.UserId,
                    UniqueName = p.UniqueName,
                    DisplayName = p.DisplayedName
                })
                .ToList() ?? new List<User>(),

            // мета чата
            ChatName = chat.ChatMeta?.Name,
            ChatType = chat.ChatMeta != null
                ? (ChatType)chat.ChatMeta.Type
                : default,
            LastMessageNum = chat.ChatMeta?.LastMessageNum ?? 0
        };
    }
}