namespace Main.BL.Models;

public class ChatParticipant(Guid userId, 
    ulong lastMessageRead)
{
    public Guid UserId { get; } = userId;
    public ulong LastMessageRead { get; } = lastMessageRead;
}
