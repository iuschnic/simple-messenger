using Main.BL.Enums;
namespace Main.BL.Models;

public class Chat
{
    private Chat(
        Guid id,
        string? name,
        ChatType type,
        Guid? ownerUserId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum,
        List<ChatUser> participants)
    {
        switch (type)
        {
            case ChatType.Private:
                if (participants.Count != 2)
                    throw new ArgumentException("Private chat must have exactly 2 participants");
                break;
            case ChatType.Group:
                if (participants.Count < 1)
                    throw new ArgumentException("Group must have at least 1 participant");
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Group chat must have a no-null name");
                break;
        }
        Id = id;
        Name = type == ChatType.Private ? null : name;
        Type = type;
        OwnerUserId = type == ChatType.Private ? null : ownerUserId;
        CreatedAt = createdAt;
        Version = version;
        LastMessageNum = lastMessageNum;
        Participants = participants.AsReadOnly();
    }

    public static Chat CreateGroup(
        string name,
        Guid ownerUserId,
        List<ChatUser> participants)
    {
        return new Chat(Guid.NewGuid(),
            name,
            ChatType.Group,
            ownerUserId,
            DateTime.UtcNow,
            0,
            0,
            participants);
    }

    public static Chat CreatePrivate(
        ChatUser participant1, ChatUser participant2)
    {
        return new Chat(Guid.NewGuid(),
            null,
            ChatType.Private,
            null,
            DateTime.UtcNow,
            0,
            0,
            [participant1, participant2]);
    }

    public static Chat Create(
        Guid id,
        string? name,
        ChatType type,
        Guid? ownerUserId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum,
        List<ChatUser> participants)
    {
        return new Chat(id,
            name,
            type,
            ownerUserId,
            createdAt,
            version,
            lastMessageNum,
            participants);
    }

    public Guid Id { get; }
    public string? Name { get; }  //у приватного чата нет названия, у публичного есть
    public ChatType Type { get; }
    public Guid? OwnerUserId { get; }  //может быть удален + у личного чата нет владельца
    public DateTime CreatedAt { get; }
    public ulong Version { get; }
    public ulong LastMessageNum { get; }
    public IReadOnlyList<ChatUser> Participants { get; }
}
