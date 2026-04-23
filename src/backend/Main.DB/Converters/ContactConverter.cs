using Main.BL.Models;
using Main.DB.Models;

namespace Main.DB.Converters;

public static class ContactConverter
{
    public static ContactDb ToDb(this Contact domain, Guid ownerUserId)
    {
        return new ContactDb(
            ownerUserId,
            domain.ContactUserId,
            domain.ContactName);
    }

    public static Contact ToDomain(this ContactDb db)
    {
        return new Contact(
            db.ContactUserId,
            db.ContactName);
    }
}
