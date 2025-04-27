using DrowsinessDetectionServer.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace DrowsinessDetectionServer.Datas;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Supervisor> Supervisors { get; set; } = null!;
    public DbSet<Driver> Drivers { get; set; } = null!;
    public DbSet<FaceData> FaceDatas { get; set; } = null!;
    public DbSet<MonitorSession> MonitorSessions { get; set; } = null!;
    public DbSet<DetectionLog> DetectionLogs { get; set; } = null!;
    public DbSet<NotificationLog> NotificationLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Supervisor>().ToTable("Supervisors");
        modelBuilder.Entity<Driver>().ToTable("Drivers");
        modelBuilder.Entity<FaceData>().ToTable("FaceDatas");
        modelBuilder.Entity<MonitorSession>().ToTable("MonitorSessions");
        modelBuilder.Entity<DetectionLog>().ToTable("DetectionLogs");
        modelBuilder.Entity<NotificationLog>().ToTable("NotificationLogs");

        modelBuilder.Entity<User>()
            .Property(u => u.UserName)
            .UseCollation("Latin1_General_CS_AS");
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UserName)
            .HasFilter("UserName <> ''")
            .IsUnique();

        modelBuilder.Entity<Driver>()
            .HasOne(d => d.Supervisor)
            .WithMany(s => s.Drivers)
            .HasForeignKey(d => d.SupervisorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Driver>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<Driver>(d => d.Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Supervisor>()
            .HasOne<User>()
            .WithOne()
            .HasForeignKey<Supervisor>(s => s.Id)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Driver>()
            .HasOne(d => d.FaceData)
            .WithOne(fd => fd.Driver)
            .HasForeignKey<FaceData>(fd => fd.DriverId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MonitorSession>()
            .HasOne(ms => ms.Driver)
            .WithMany(d => d.MonitorSessions)
            .HasForeignKey(ms => ms.DriverId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DetectionLog>()
            .HasOne(dl => dl.Driver)
            .WithMany(d => d.DetectionLogs)
            .HasForeignKey(dl => dl.DriverId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<DetectionLog>()
            .HasOne(dl => dl.Session)
            .WithMany(ms => ms.DetectionLogs)
            .HasForeignKey(dl => dl.SessionId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<NotificationLog>()
            .HasOne(nl => nl.Supervisor)
            .WithMany(s => s.NotificationLogs)
            .HasForeignKey(nl => nl.SupervisorId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<NotificationLog>()
            .HasOne(nl => nl.Detection)
            .WithMany(dl => dl.NotificationLogs)
            .HasForeignKey(nl => nl.DetectionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
