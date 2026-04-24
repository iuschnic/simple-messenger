using Main.BL.Models;
using Main.DB.Models;

namespace Main.DB.Converters;

public static class ChatUserConverter
{
    public static ChatUserDb ToDb(this ChatUser domain, Guid chatId)
    {
        return new ChatUserDb(
            chatId,
            domain.UserId,
            domain.LastMessageRead);
    }

    public static ChatUser ToDomain(this ChatUserDb db)
    {
        return new ChatUser(
            db.UserId,
            db.LastMessageRead);
    }
}
