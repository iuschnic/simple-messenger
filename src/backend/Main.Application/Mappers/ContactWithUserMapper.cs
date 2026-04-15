using Main.Application.Dtos;
using Main.BL.Models;
namespace Main.Application.Mappers;

public static class ContactWithUserMapper
{
    public static ContactWithUserDto ToDto(this Contact domain, User user)
    {
        if (domain.ContactUserId != user.Id)
            throw new ArgumentException(
                $"User Id mismatch: Contact.ContactUserId ({domain.ContactUserId}) " +
                $"does not match User.Id ({user.Id})");
        return new ContactWithUserDto
        {
            ContactUser = user,
            ContactName = domain.ContactName
        };
    }
}
