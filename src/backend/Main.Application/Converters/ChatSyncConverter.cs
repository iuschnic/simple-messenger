using Main.BL.Models;

namespace Main.Application.Converters;

public static class ChatSyncConverter
{
    public static ChatSync ToSync(this Chat chat, List<User> users, List<Message> messages, ChatSyncStatus status)
    {
        return new ChatSync
        {
            Status = status,
            ChatMeta = new ChatMeta
            {
                Id = chat.Id,
                Name = chat.Name,
                Type = chat.Type,
                OwnerUserId = chat.OwnerUserId,
                CreatedAt = chat.CreatedAt,
                Version = chat.Version,
                LastMessageNum = chat.LastMessageNum
            },
            Participants = chat.Participants
                .Select(p =>
                {
                    var user = users.First(u => u.Id == p.UserId);
                    return new ChatParticipantInfo
                    {
                        UserId = user.Id,
                        UniqueName = user.UniqueName,
                        DisplayedName = user.DisplayedName,
                        LastMessageRead = p.LastMessageRead
                    };
                }).ToList(),
            Messages = messages
        };
    }
}
