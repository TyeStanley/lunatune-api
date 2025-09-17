namespace Models;

public class PlaylistWithSongDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = default!;
  public string? Description { get; set; }
  public Guid CreatorId { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public bool IsCreator { get; set; }
  public CreatorInfoDto Creator { get; set; } = default!;
  public List<Song> Songs { get; set; } = [];
}