namespace Lunatune.Core.Models;

public class Playlist
{
  public Guid Id { get; set; }
  public required string Name { get; set; }
  public string? Description { get; set; }
  public required Guid UserId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }

  // Navigation properties
  public User User { get; set; } = null!;
  public ICollection<PlaylistSong> Songs { get; set; } = [];
}