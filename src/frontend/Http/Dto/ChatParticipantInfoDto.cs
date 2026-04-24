namespace Http.Dto;

public class ChatParticipantInfoDto
{
    public Guid UserId { get; set; }
    public string UniqueName { get; set; } 
    public string DisplayedName { get; set; } = string.Empty;
    public ulong LastMessageRead { get; set; }
}