namespace BL.Models;

public class User
{
    public Guid Id { get; set; }
    public string UniqueName { get; set; }
    public string DisplayName { get; set; }
    public string? ContactName { get; set; }
}