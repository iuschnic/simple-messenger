using Main.BL.Exceptions;

namespace Main.BL.Models;

public class User
{
    public const int UniqueNameMaxLength = 64;
    public const int DisplayedNameMaxLength = 128;
    public Guid Id { get; }
    public string UniqueName { get; }
    public string DisplayedName { get; private set; }
    private User(Guid id,
        string uniqueName,
        string displayedName)
    {
        CheckUniqueName(uniqueName);
        CheckDisplayedName(displayedName);
        CheckId(id);
        Id = id;
        UniqueName = uniqueName;
        DisplayedName = displayedName;
    }
    private static void CheckUniqueName(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName))
            throw new DomainValidationException("Empty unique name");
        if (uniqueName.Length > UniqueNameMaxLength)
            throw new DomainValidationException($"Unique name cannot exceed {UniqueNameMaxLength} characters");
    }
    private static void CheckDisplayedName(string displayedName)
    {
        if (string.IsNullOrWhiteSpace(displayedName))
            throw new DomainValidationException("Empty displayed name");
        if (displayedName.Length > DisplayedNameMaxLength)
            throw new DomainValidationException($"Displayed name cannot exceed {DisplayedNameMaxLength} characters");
    }
    private static void CheckId(Guid id)
    {
        if (id != Guid.Empty)
            throw new DomainValidationException("Empty id");
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
    public void ChangeDisplayedName(string newDisplayedName)
    {
        CheckDisplayedName(newDisplayedName);
        DisplayedName = newDisplayedName;
    }
}
