using Main.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.DB.Context.Configurations;

// гипотетически могут возникнуть проблемы из-за двух каскадных удалений,
// кажется это не во всех БД разрешено, но Postgres должен сработать 
public class ContactDbConfiguration : IEntityTypeConfiguration<ContactDb>
{
    public void Configure(EntityTypeBuilder<ContactDb> builder)
    {
        builder.HasKey(c => new { c.OwnerUserId, c.ContactUserId });

        // Каскадное удаление контактов при удалении пользователя-владельца контактов
        builder.HasOne(c => c.OwnerUser)
            .WithMany(u => u.Contacts)
            .HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Каскадное удаление контактов при удалении пользователя-контакта
        builder.HasOne(c => c.ContactUser)
            .WithMany()
            .HasForeignKey(c => c.ContactUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
