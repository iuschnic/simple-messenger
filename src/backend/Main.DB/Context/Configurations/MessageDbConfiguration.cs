using Main.DB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Main.DB.Context.Configurations;

public class MessageDbConfiguration: IEntityTypeConfiguration<MessageDb>
{
    public void Configure(EntityTypeBuilder<MessageDb> builder)
    {
        builder.HasKey(m => new { m.MessageNumber, m.ChatId });

        // Каскадное удаление сообщений при удалении чата
        builder.HasOne(m => m.Chat)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        // Проставление null в SenderUserId при удалении пользователя-отправителя сообщения
        builder.HasOne(m => m.SenderUser)
            .WithMany()
            .HasForeignKey(m => m.SenderUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Проставление null в ForwardedFromUserId при удалении пользователя-отправителя оригинального сообщения
        builder.HasOne(cu => cu.ForwardedFromUser)
            .WithMany()
            .HasForeignKey(cu => cu.ForwardedFromUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Проставление null в ReplyToMessageNumber при удалении оригинального сообщения
        builder.HasOne(m => m.ReplyToMessage)
            .WithMany()
            .HasForeignKey(m => new { m.ReplyToMessageNumber, m.ChatId })
            .OnDelete(DeleteBehavior.SetNull);
    }
}
