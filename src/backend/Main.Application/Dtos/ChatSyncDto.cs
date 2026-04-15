using Main.BL.Enums;
using Main.BL.Models;

namespace Main.Application.Dtos;
public class ChatMeta
{
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

public enum ChatSyncStatus
{
    // Чат существует и клиент о нем знает (обычная синхронизация)
    Synced = 0,
    // Новый чат: клиент не знал о нем
    New = 1,
    // Чат удален: клиент знал о чате, но на сервере этой связи уже нет (удален или пользователь вышел из него)
    Deleted = 2
}

public class ChatSyncDto
{
    // Id чата
    public Guid ChatId { get; init; }
    // Статус чата
    public ChatSyncStatus Status { get; init; }
    // Мета-информация о чате
    public ChatMeta? ChatMeta { get; init; }
    // Список новых/удаленных/измененных сообщений
    public List<Message>? Messages { get; init; }
    // Список участников чата, включая информацию о последних прочитанных сообщениях
    public List<ChatParticipantInfo>? Participants { get; init; }
}
