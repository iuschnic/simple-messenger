namespace Main.BL.Models;

public class User(Guid id, 
    string uniqueName, 
    string displayedName)
{
    public Guid Id { get; } = id;
    public string UniqueName { get; } = uniqueName;
    public string DisplayedName { get; } = displayedName;
}
