using System.ComponentModel.DataAnnotations;

namespace Main.DB.Models;

public class UserDb
{
    internal UserDb() { }
    public UserDb(Guid id, string uniqueName, string displayedName)
    {
        Id = id;
        UniqueName = uniqueName;
        DisplayedName = displayedName;
    }
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string UniqueName { get; set; }
    [Required]
    public string DisplayedName { get; set; }

    public ICollection<ContactDb> Contacts { get; set; } = [];
    public ICollection<ChatUserDb> UserChats { get; set; } = [];
    public ICollection<MessageDb> SentMessages { get; set; } = [];
}
