using Lunatune.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Lunatune.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Song> Songs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<SongLike> SongLikes { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistSong> PlaylistSongs { get; set; }
    public DbSet<UserLibraryPlaylist> UserLibraryPlaylists { get; set; }

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

        // SongLike entity
        modelBuilder.Entity<SongLike>(entity =>
        {
            entity.Property(sl => sl.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(sl => sl.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure unique constraint for user-song combination
            entity.HasIndex(sl => new { sl.UserId, sl.SongId })
                .IsUnique();

            // Configure relationships
            entity.HasOne(sl => sl.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sl => sl.Song)
                .WithMany(s => s.Likes)
                .HasForeignKey(sl => sl.SongId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Playlist entity
        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.Property(p => p.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(p => p.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(500);

            // Configure relationships
            entity.HasOne(p => p.Creator)
                .WithMany(u => u.CreatedPlaylists)
                .HasForeignKey(p => p.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PlaylistSong entity
        modelBuilder.Entity<PlaylistSong>(entity =>
        {
            entity.Property(ps => ps.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(ps => ps.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure unique constraint for playlist-song combination
            entity.HasIndex(ps => new { ps.PlaylistId, ps.SongId })
                .IsUnique();

            // Configure relationships
            entity.HasOne(ps => ps.Playlist)
                .WithMany(p => p.Songs)
                .HasForeignKey(ps => ps.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ps => ps.Song)
                .WithMany()
                .HasForeignKey(ps => ps.SongId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserLibraryPlaylist entity
        modelBuilder.Entity<UserLibraryPlaylist>(entity =>
        {
            entity.Property(ul => ul.Id)
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(ul => ul.AddedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Configure unique constraint for user-playlist combination
            entity.HasIndex(ul => new { ul.UserId, ul.PlaylistId })
                .IsUnique();

            // Configure relationships
            entity.HasOne(ul => ul.User)
                .WithMany(u => u.LibraryPlaylists)
                .HasForeignKey(ul => ul.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ul => ul.Playlist)
                .WithMany(p => p.LibraryEntries)
                .HasForeignKey(ul => ul.PlaylistId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}