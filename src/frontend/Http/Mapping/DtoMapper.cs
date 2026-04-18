using BL.Models;
using Http.Dto;

namespace Http.Mapping;

public static class DtoMapper
{
    public static User ToUser(UserDto d) => new()
    {
        Id = d.Id,
        UniqueName = d.UniqueName,
        DisplayName = d.DisplayName
    };
    
    public static CurrentUser ToCurrentUser(CurrentUserDto d) => new()
    {
        UniqueName = d.UniqueName,
        PasswordHash= d.PasswordHash,
        DisplayedName = d.DisplayedName,
        Email = d.Email,
    };
    
    public static Chat ToChat(ChatDto d) => new()
    {
        Id = d.ChatId,
        Name = d.Name,
        OwnerId = d.OwnerId,
        CreatedAt = d.CreatedAt,
        Version = d.Version,
        Type = d.Type,
        LastMessageNum = d.LastMessageNum,
        Members = d.Members?
            .Select(ToUser)
            .ToList() ?? new List<User>()
    };

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
        Type = d.Type
    };

    public static SyncChatResult ToSync(SyncChatResponseDto d) => new()
    {
        ChatId = d.ChatId,
        LastVersion = d.LastVersion,
        Messages = d.Messages.Select(ToMessage).ToList()
    };
}