using Main.DB.Enums;
using System.ComponentModel.DataAnnotations;

namespace Main.DB.Models;

public class ChatDb
{
    internal ChatDb() { }
    public ChatDb(Guid id,
        string name,
        ChatTypeDb type,
        Guid ownerId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum)
    {
        Id = id; 
        Name = name; 
        Type = type; 
        OwnerId = ownerId;
        CreatedAt = DateTime.UtcNow;
        Version = version;
        LastMessageNum = lastMessageNum;
    }
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public ChatTypeDb Type { get; set; }
    [Required]
    public Guid OwnerId { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public ulong Version { get; set; }  
    [Required]
    public ulong LastMessageNum { get; set; }

    public UserDb? Owner {  get; set; }
    public ICollection<ChatUserDb> Participants { get; set; } = [];
    public ICollection<MessageDb> Messages { get; set; } = [];
    
}
