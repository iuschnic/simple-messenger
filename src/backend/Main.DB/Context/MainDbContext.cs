using Main.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Main.DB.Context;

public class MainDbContext : DbContext
{
    public DbSet<UserDb> Users { get; set; }
    public DbSet<MessageDb> Messages { get; set; }
    public DbSet<ContactDb> Contacts { get; set; }
    public DbSet<ChatUserDb> ChatsUsers { get; set; }
    public DbSet<ChatDb> Chats {  get; set; }

    public MainDbContext(DbContextOptions<MainDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MainDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
