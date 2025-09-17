namespace Models;

public class User
{
    public Guid Id { get; set; }
    public required string Auth0Id { get; set; }
    public required string Email { get; set; }
    public string? Name { get; set; }
    public string? Picture { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Likes relationship
    public ICollection<SongLike> Likes { get; set; } = [];

    // Navigation properties
    public ICollection<Playlist> CreatedPlaylists { get; set; } = [];
    public ICollection<UserLibraryPlaylist> LibraryPlaylists { get; set; } = [];
}
