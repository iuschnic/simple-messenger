using Main.BL.Models;

namespace Main.Application.Dtos;

public class ContactWithUserDto
{
    public UserDto ContactUser { get; init; }
    public string ContactName { get; init; }
}
