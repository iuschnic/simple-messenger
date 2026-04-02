using Main.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.DB.Context.Configurations;

public class ChatUserDbConfiguration : IEntityTypeConfiguration<ChatUserDb>
{
    public void Configure(EntityTypeBuilder<ChatUserDb> builder)
    {
        builder.HasKey(cu => new {cu.UserId, cu.ChatId});

        // Каскадное удаление связок чат-пользователь при удалении чата
        builder.HasOne(cu => cu.Chat)
            .WithMany(c => c.ChatUsers)
            .HasForeignKey(cu => cu.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // Каскадное удаление связок чат-пользователь при удалении пользователя
        builder.HasOne(cu => cu.User)
            .WithMany(c => c.UserChats)
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
