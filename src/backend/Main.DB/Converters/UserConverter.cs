using Main.BL.Models;
using Main.DB.Models;

namespace Main.DB.Converters;

public static class UserConverter
{
    public static UserDb ToDb(this User domain)
    {
        return new UserDb(
            domain.Id,
            domain.UniqueName,
            domain.DisplayedName);
    }

    public static User ToDomain(this UserDb db)
    {
        return User.Create(
            db.Id,
            db.UniqueName,
            db.DisplayedName);
    }
}
