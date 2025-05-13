namespace Lunatune.Core.Models;

public class SongWithLikeInfo
{
  public Song Song { get; set; } = null!;
  public bool IsLiked { get; set; }
  public int LikeCount { get; set; }
}