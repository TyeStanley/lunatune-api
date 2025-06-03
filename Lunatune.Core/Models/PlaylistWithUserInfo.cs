namespace Lunatune.Core.Models;

public class PlaylistWithUserInfo
{
  public Guid Id { get; set; }
  public required string Name { get; set; }
  public string? Description { get; set; }
  public required Guid CreatorId { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime? UpdatedAt { get; set; }
  public bool IsPublic { get; set; }
  public bool IsCreator { get; set; }
  public bool IsInLibrary { get; set; }

  // Navigation properties
  public CreatorInfoDto? Creator { get; set; }
}