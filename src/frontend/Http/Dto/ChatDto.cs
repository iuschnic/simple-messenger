using BL.Models;
using Http.Dto;


public class ChatDto
{
    public Guid ChatId { get; set; }
    public string Name { get; set; }
    public int Type { get; set; }
    public Guid OwnerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public ulong Version { get; set; }
    public ulong LastMessageNum { get; set; }

    public List<UserDto> Members { get; set; } = new();
}