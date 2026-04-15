using Main.Application.Dtos;
using Main.BL.Models;
namespace Main.Application.Mappers;

public static class ChatSyncMapper
{
    public static ChatSyncDto ToDto(
        this Chat domain,
        ChatSyncStatus status,
        List<ChatParticipantInfo>? participants = null,
        List<Message>? messages = null)
    {
        var isDeletedOrLeft = status == ChatSyncStatus.Deleted || status == ChatSyncStatus.Left;
        return new ChatSyncDto
        {
            ChatId = domain.Id,
            Status = status,
            ChatMeta = isDeletedOrLeft ? null : new ChatMeta
            {
                Name = domain.Name,
                Type = domain.Type,
                OwnerUserId = domain.OwnerUserId,
                CreatedAt = domain.CreatedAt,
                Version = domain.Version,
                LastMessageNum = domain.LastMessageNum
            },
            Messages = isDeletedOrLeft ? null : messages,
            Participants = isDeletedOrLeft ? null : participants
        };
    }
}