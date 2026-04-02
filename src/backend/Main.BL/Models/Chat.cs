using Main.BL.Enums;
namespace Main.BL.Models;

public class Chat
{
    public Chat(
        Guid id,
        string name,
        ChatType type,
        Guid? ownerId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum,
        List<ChatParticipant> participants)
    {
        switch (type)
        {
            case ChatType.Private:
                if (ownerId != null)
                    throw new ArgumentException("Private chat cannot have an owner");
                if (participants.Count != 2)
                    throw new ArgumentException("Private chat must have exactly 2 participants");
                break;

            case ChatType.Group:
                if (participants.Count < 2)
                    throw new ArgumentException("Group must have at least 1 participant");
                break;
        }

        Id = id;
        Name = name;
        Type = type;
        OwnerId = ownerId;
        CreatedAt = createdAt;
        Version = version;
        LastMessageNum = lastMessageNum;
        Participants = participants;
    }

    public Guid Id { get; }
    public string Name { get; }
    public ChatType Type { get; }
    public Guid? OwnerId { get; }  //может быть удален + у личного чата нет владельца
    public DateTime CreatedAt { get; }
    public ulong Version { get; }
    public ulong LastMessageNum { get; }
    public IReadOnlyList<ChatParticipant> Participants { get; }
}
