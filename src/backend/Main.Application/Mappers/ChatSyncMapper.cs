using Main.Application.Dtos;
using Main.BL.Models;
using Main.Application.Enums;
namespace Main.Application.Mappers;

public static class ChatSyncMapper
{
    public static ChatSyncDto ToDto(
        this Chat domain,
        ChatSyncStatus status,
        List<ChatParticipantInfo>? participants = null,
        List<Message>? messages = null)
    {
        return new ChatSyncDto
        {
            ChatId = domain.Id,
            Status = status,
            ChatMeta = status == ChatSyncStatus.Deleted ? null : new ChatMetaDto
            {
                Name = domain.Name,
                Type = domain.Type,
                OwnerUserId = domain.OwnerUserId,
                CreatedAt = domain.CreatedAt,
                Version = domain.Version,
                LastMessageNum = domain.LastMessageNum
            },
            Messages = status == ChatSyncStatus.Deleted ? null : messages,
            Participants = status == ChatSyncStatus.Deleted ? null : participants
        };
    }
}