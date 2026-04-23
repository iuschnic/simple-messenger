using Main.BL.Models;
using Main.Application.Dtos;

namespace Main.Application.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(this User domain)
    {
        return new UserDto 
        { 
            Id = domain.Id,
            UniqueName = domain.UniqueName,
            DisplayedName = domain.DisplayedName,
        };
    }
}
