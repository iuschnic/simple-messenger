using Main.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.DB.Context.Configurations;

public class UserDbConfiguration : IEntityTypeConfiguration<UserDb>
{
    public void Configure(EntityTypeBuilder<UserDb> builder)
    {
        builder.HasKey(c => c.Id);

        // Каскадное удаление связок чат-пользователь при удалении пользователя
        builder.HasMany(u => u.UserChats)
            .WithOne(uc => uc.User)
            .HasForeignKey(uc => uc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Каскадное удаление контатков при удалении пользователя
        builder.HasMany(u => u.Contacts)
            .WithOne(c => c.OwnerUser)
            .HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}