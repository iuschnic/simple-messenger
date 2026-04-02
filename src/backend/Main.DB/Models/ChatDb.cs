using Main.DB.Enums;
using System.ComponentModel.DataAnnotations;

namespace Main.DB.Models;

public class ChatDb
{
    internal ChatDb() { }
    public ChatDb(Guid id,
        string name,
        ChatTypeDb type,
        Guid? ownerUserId,
        DateTime createdAt,
        ulong version,
        ulong lastMessageNum)
    {
        Id = id; 
        Name = name; 
        Type = type; 
        OwnerUserId = ownerUserId;
        CreatedAt = createdAt;
        Version = version;
        LastMessageNum = lastMessageNum;
    }
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public ChatTypeDb Type { get; set; }
    public Guid? OwnerUserId { get; set; }    // пользователь может быть удален
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public ulong Version { get; set; }  
    [Required]
    public ulong LastMessageNum { get; set; }

    public UserDb? OwnerUser {  get; set; }
    public ICollection<ChatUserDb> ChatUsers { get; set; } = [];
    public ICollection<MessageDb> Messages { get; set; } = [];
    
}
