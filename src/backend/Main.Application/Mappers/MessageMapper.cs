using Main.Application.Enums;
using Main.BL.Models;
using Main.Application.Dtos;

namespace Main.Application.Mappers;

public static class MessageMapper
{
    public static MessageDto ToDto(this Message domain)
    {
        return new MessageDto
        {
            MessageNumber = domain.MessageNumber,
            ChatId = domain.ChatId,
            SenderUserId = domain.SenderUserId,
            Text = domain.Text,
            CreatedAt = domain.CreatedAt,
            EditedAt = domain.EditedAt,
            Deleted = domain.Deleted,
            Version = domain.Version,
            Type = (MessageTypeApp) domain.Type,
            ReplyToMessageNumber = domain.ReplyToMessageNumber,
            ForwardedFromUserId = domain.ForwardedFromUserId
        };
    }
}