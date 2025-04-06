using DrowsinessDetectionServer.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace DrowsinessDetectionServer.Datas;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> User { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(u => u.UserName)
            .UseCollation("Latin1_General_CS_AS");
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .HasFilter("UserName <> ''")
            .IsUnique();
    }
}
