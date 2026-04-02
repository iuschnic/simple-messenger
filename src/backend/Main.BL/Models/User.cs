using System;

namespace Main.BL.Models;

public class User
{
    private User(Guid id,
        string uniqueName,
        string displayedName)
    {
        Id = id;
        UniqueName = uniqueName;
        DisplayedName = displayedName;
    }
    public static User CreateNew(string uniqueName,
        string displayedName)
    {
        return new User(Guid.NewGuid(), uniqueName, displayedName);
    }
    public static User Create(Guid id,
        string uniqueName,
        string displayedName)
    {
        return new User(id, uniqueName, displayedName);
    }
    public Guid Id { get; }
    public string UniqueName { get; }
    public string DisplayedName { get; }
}
