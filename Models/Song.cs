namespace Lunatune.Models;

public class Song
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Artist { get; set; }
    public string? Album { get; set; }
    public string? Genre { get; set; }

    // File essentials
    public required string FilePath { get; set; }
    public long DurationMs { get; set; }
    public string? AlbumArtUrl { get; set; }

    // Basic user interaction
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Likes relationship
    public ICollection<SongLike> Likes { get; set; } = [];
}
