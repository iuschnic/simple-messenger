using Main.BL.Models;

namespace Main.Application.Dtos;

public class ContactWithUser
{
    public User ContactUser { get; init; }
    public string ContactName { get; init; }
}
