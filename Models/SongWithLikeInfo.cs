namespace Models;

public class SongWithLikeInfo
{
  public Guid Id { get; set; }
  public string Title { get; set; } = null!;
  public string Artist { get; set; } = null!;
  public string? Album { get; set; }
  public string? Genre { get; set; }
  public string FilePath { get; set; } = null!;
  public long DurationMs { get; set; }
  public string? AlbumArtUrl { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public bool IsLiked { get; set; }
  public int LikeCount { get; set; }
}