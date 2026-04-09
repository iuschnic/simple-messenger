using Main.BL.Enums;

namespace Main.BL.Models;
public class ChatMeta
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public ChatType Type { get; init; }
    public Guid? OwnerUserId { get; init; }
    public DateTime CreatedAt { get; init; }
    public ulong Version { get; init; }
    public ulong LastMessageNum { get; init; }
}

public class ChatParticipantInfo
{
    public Guid UserId { get; init; }
    public string UniqueName { get; init; }
    public string DisplayedName { get; init; }
    public ulong LastMessageRead { get; init; }
}

public class ChatSync
{
    // Мета-информация о чате
    public ChatMeta ChatMeta { get; init; }
    // Список новых/удаленных/измененных сообщений
    public List<Message> Messages { get; init; } = new();
    // Список участников чата, включая информацию о последних прочитанных сообщениях
    public List<ChatParticipantInfo> Participants { get; init; } = new();
}
