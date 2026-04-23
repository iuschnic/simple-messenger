using Main.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.DB.Context.Configurations;

public class ChatDbConfiguration : IEntityTypeConfiguration<ChatDb>
{
    public void Configure(EntityTypeBuilder<ChatDb> builder)
    {
        builder.HasKey(c => c.Id);

        // Каскадное удаление связок чат-пользователь при удалении чата
        builder.HasMany(c => c.ChatUsers)
            .WithOne(cu => cu.Chat)
            .HasForeignKey(cu => cu.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // Выставление null в OwnerUser при удалении владельца чата
        builder.HasOne(c => c.OwnerUser)
            .WithMany()
            .HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Каскадное удаление сообщений при удалении чата
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.RowVersion).IsRowVersion();
    }
}
