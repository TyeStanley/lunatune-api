namespace Models;

public class PlaylistSong
{
  public Guid Id { get; set; }
  public required Guid PlaylistId { get; set; }
  public required Guid SongId { get; set; }
  public int Position { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation properties
  public Playlist Playlist { get; set; } = null!;
  public Song Song { get; set; } = null!;
}