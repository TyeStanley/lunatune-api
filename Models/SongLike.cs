namespace Models;

public class SongLike
{
  public Guid Id { get; set; }
  public required Guid UserId { get; set; }
  public required Guid SongId { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  // Navigation properties
  public User User { get; set; } = null!;
  public Song Song { get; set; } = null!;
}