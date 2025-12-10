using IdAnimal.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace IdAnimal.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Establishment> Establishments { get; set; }
    public DbSet<Cattle> Cattle { get; set; }
    public DbSet<CattleImage> CattleImages { get; set; }
    public DbSet<CattleFullImage> CattleFullImages { get; set; }
    public DbSet<CattleVideo> CattleVideos { get; set; }
    public DbSet<CustomDataColumn> CustomDataColumns { get; set; }
    public DbSet<CustomDataValue> CustomDataValues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
        });

        // Establishment configuration
        modelBuilder.Entity<Establishment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Cattle configuration
        modelBuilder.Entity<Cattle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Caravan).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Caravan);
            entity.HasOne(e => e.Establishment)
                .WithMany(e => e.Cattle)
                .HasForeignKey(e => e.EstablishmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CattleImage configuration
        modelBuilder.Entity<CattleImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Path).IsRequired();
            entity.HasOne(e => e.Cattle)
                .WithMany(c => c.Images)
                .HasForeignKey(e => e.CattleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CattleFullImage configuration
        modelBuilder.Entity<CattleFullImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Path).IsRequired();
            entity.HasOne(e => e.Cattle)
                .WithMany(c => c.FullImages)
                .HasForeignKey(e => e.CattleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CattleVideo configuration
        modelBuilder.Entity<CattleVideo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Path).IsRequired();
            entity.HasOne(e => e.Cattle)
                .WithMany(c => c.Videos)
                .HasForeignKey(e => e.CattleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CustomDataColumn configuration
        modelBuilder.Entity<CustomDataColumn>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ColumnName).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CustomDataValue configuration
        modelBuilder.Entity<CustomDataValue>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.CustomDataColumn)
                .WithMany(c => c.Values)
                .HasForeignKey(e => e.CustomDataColumnId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Cattle)
                .WithMany(c => c.CustomDataValues)
                .HasForeignKey(e => e.CattleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
