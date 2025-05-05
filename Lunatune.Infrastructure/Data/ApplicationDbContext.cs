using Lunatune.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Lunatune.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Song> Songs { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Song entity
        modelBuilder.Entity<Song>(entity =>
        {
            entity.Property(s => s.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(s => s.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.Artist)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.Album)
                .HasMaxLength(200);

            entity.Property(s => s.FilePath)
                .IsRequired();

            entity.Property(s => s.DurationMs)
                .IsRequired();

            entity.Property(s => s.Genre)
                .HasMaxLength(100);
        });

        // User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(u => u.Auth0Id)
                .IsRequired();

            entity.Property(u => u.Email)
                .IsRequired();

            entity.HasIndex(u => u.Auth0Id)
                .IsUnique();
        });
    }
}