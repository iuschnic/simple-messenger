using Main.BL.Enums;
namespace Main.BL.Models;

public class Chat(Guid id,
    string name,
    ChatType type,
    Guid ownerId,
    DateTime createdAt,
    ulong version,
    ulong lastMessageNum,
    List<ChatParticipant> participants
    )
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public ChatType Type { get; } = type;
    public Guid OwnerId { get; } = ownerId;
    public DateTime CreatedAt { get; } = createdAt;
    public ulong Version { get; } = version;
    public ulong LastMessageNum { get; } = lastMessageNum;
    public List<ChatParticipant> Participants { get; } = participants;
}
