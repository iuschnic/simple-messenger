using Auth.BL.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.DB.Postgres
{
    public class ServerDbContext : DbContext
    {
        public ServerDbContext(DbContextOptions<ServerDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Дополнительная настройка маппинга, если атрибутов недостаточно.
            // Например, можно задать индексы:
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UniqueName)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}