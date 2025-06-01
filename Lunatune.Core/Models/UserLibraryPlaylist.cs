namespace Lunatune.Core.Models;

public class UserLibraryPlaylist
{
  public Guid Id { get; set; }
  public required Guid UserId { get; set; }
  public required Guid PlaylistId { get; set; }
  public DateTime AddedAt { get; set; } = DateTime.UtcNow;

  // Navigation properties
  public User User { get; set; } = null!;
  public Playlist Playlist { get; set; } = null!;
}