using Main.BL.Models;
using Main.BL.Enums;
using Main.DB.Models;
using Main.DB.Enums;

namespace Main.DB.Converters;

public static class ChatConverter
{
    public static ChatDb ToDb(this Chat domain)
    {
        return new ChatDb(
            domain.Id,
            domain.Name,
            (ChatTypeDb)domain.Type,
            domain.OwnerUserId,
            domain.CreatedAt,
            domain.Version,
            domain.LastMessageNum);
    }

    public static Chat ToDomain(this ChatDb db, List<ChatUserDb> participants)
    {
        return Chat.Create(
            db.Id,
            db.Name,
            (ChatType)db.Type,
            db.OwnerUserId,
            db.CreatedAt,
            db.Version,
            db.LastMessageNum,
            [.. participants.Select(p => p.ToDomain())]);
    }
}
