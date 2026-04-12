using Main.Application.Dtos;
using Main.BL.Models;

namespace Main.Application.Converters;

public static class ContactWithUserConverter
{
    public static ContactWithUser ToContactWithUser(this Contact contact, User user)
    {
        return new ContactWithUser
        {
            ContactUser = user,
            ContactName = contact.ContactName
        };
    }
}

