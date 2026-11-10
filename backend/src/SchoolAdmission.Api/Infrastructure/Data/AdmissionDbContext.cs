using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Domain.Entities;

namespace SchoolAdmission.Api.Infrastructure.Data;

public class AdmissionDbContext : DbContext
{
    public AdmissionDbContext(DbContextOptions<AdmissionDbContext> options) : base(options)
    {
    }

    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Hold> Holds => Set<Hold>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Class>(entity =>
        {
            entity.ToTable("classes");

            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasMaxLength(64);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.TotalSeats).IsRequired();
            entity.Property(c => c.CreatedAt).IsRequired();

            entity.HasMany(c => c.Holds)
                .WithOne(h => h.Class)
                .HasForeignKey(h => h.ClassId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Hold>(entity =>
        {
            entity.ToTable("holds");

            entity.HasKey(h => h.Id);
            entity.Property(h => h.Id).HasMaxLength(64);
            entity.Property(h => h.ClassId).IsRequired().HasMaxLength(64);
            entity.Property(h => h.StudentId).IsRequired().HasMaxLength(32);
            entity.Property(h => h.Status).IsRequired().HasMaxLength(32);
            entity.Property(h => h.ExpiresAt).IsRequired();
            entity.Property(h => h.CreatedAt).IsRequired();
            entity.Property(h => h.ConfirmedAt).IsRequired(false);

            entity.HasIndex(h => new { h.ClassId, h.Status, h.ExpiresAt })
                .HasDatabaseName("ix_holds_class_status_expires");

            entity.HasIndex(h => new { h.ClassId, h.StudentId })
                .HasDatabaseName("ix_holds_class_student");

            entity.HasIndex(h => h.StudentId)
                .HasDatabaseName("ix_holds_student");
        });
    }
}
