using System.ComponentModel.DataAnnotations.Schema;
namespace Auth.BL.Models;

[Table("user")]
public class User(Guid id, string name, string email, string passwordHash)
{
    [Column("Id")]
    public Guid Id { get; init; } = id;
    [Column("UniqueName")]
    public string UniqueName { get; set; } = name;
    [Column("Email")]
    public string Email { get; set; } = email;
    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = passwordHash;
}
