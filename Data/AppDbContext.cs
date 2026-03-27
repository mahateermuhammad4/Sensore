using Microsoft.EntityFrameworkCore;
using Sensore.Models;

namespace Sensore.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<SensorFrame> SensorFrames => Set<SensorFrame>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ClinicianPatient> ClinicianPatients => Set<ClinicianPatient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasKey(u => u.UserId);

        modelBuilder.Entity<SensorFrame>()
            .HasKey(f => f.FrameId);

        modelBuilder.Entity<Alert>()
            .HasKey(a => a.AlertId);

        modelBuilder.Entity<Comment>()
            .HasKey(c => c.CommentId);

        modelBuilder.Entity<Report>()
            .HasKey(r => r.ReportId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<SensorFrame>()
            .HasOne(f => f.Patient)
            .WithMany(u => u.Frames)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SensorFrame>()
            .HasIndex(f => f.Timestamp);

        modelBuilder.Entity<SensorFrame>()
            .HasIndex(f => f.Ppi);

        modelBuilder.Entity<SensorFrame>()
            .HasIndex(f => f.ContactArea);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Parent)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Frame)
            .WithMany(f => f.Comments)
            .HasForeignKey(c => c.FrameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Alert>()
            .HasOne(a => a.Frame)
            .WithMany(f => f.Alerts)
            .HasForeignKey(a => a.FrameId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ClinicianPatient>()
            .HasKey(cp => new { cp.ClinicianId, cp.PatientId });

        modelBuilder.Entity<ClinicianPatient>()
            .HasOne(cp => cp.Clinician)
            .WithMany()
            .HasForeignKey(cp => cp.ClinicianId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ClinicianPatient>()
            .HasOne(cp => cp.Patient)
            .WithMany()
            .HasForeignKey(cp => cp.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
    }
}
