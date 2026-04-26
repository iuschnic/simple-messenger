using Main.BL.Models;
using Main.BL.Enums;
using Main.DB.Models;
using Main.DB.Enums;

namespace Main.DB.Converters;

public static class MessageConverter
{
    public static MessageDb ToDb(this Message domain)
    {
        return new MessageDb(
            domain.MessageNumber,
            domain.ChatId,
            domain.SenderUserId,
            domain.Text,
            domain.CreatedAt,
            domain.EditedAt,
            domain.Deleted,
            domain.Version,
            (MessageTypeDb) domain.Type,
            domain.ReplyToMessageNumber,
            domain.ForwardedFromUserId);
    }

    public static Message ToDomain(this MessageDb db)
    {
        return Message.Create(
            db.MessageNumber,
            db.ChatId,
            db.SenderUserId,
            db.Text,
            db.CreatedAt,
            db.EditedAt,
            db.Deleted,
            db.Version,
            (MessageType) db.Type,
            db.ReplyToMessageNumber,
            db.ForwardedFromUserId);
    }
}
