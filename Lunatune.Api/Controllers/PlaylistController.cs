using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;
using System.Security.Claims;

namespace Lunatune.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaylistController(IPlaylistService playlistService, ISongLikeService songLikeService) : ControllerBase
{
  private readonly IPlaylistService _playlistService = playlistService;
  private readonly ISongLikeService _songLikeService = songLikeService;

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Playlist>>> GetUserPlaylists()
  {
    var userId = GetUserId();
    var playlists = await _playlistService.GetUserPlaylistsAsync(userId);
    return Ok(playlists);
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<Playlist>> GetPlaylist(Guid id)
  {
    var userId = GetUserId();
    var playlist = await _playlistService.GetPlaylistByIdAsync(id, userId);

    if (playlist == null)
      return NotFound();

    return Ok(playlist);
  }

  [HttpPost]
  public async Task<ActionResult<Playlist>> CreatePlaylist([FromBody] CreatePlaylistRequest request)
  {
    var userId = GetUserId();
    var playlist = await _playlistService.CreatePlaylistAsync(userId, request.Name, request.Description);
    return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, playlist);
  }

  [HttpPost("{playlistId}/songs/{songId}")]
  public async Task<ActionResult> AddSongToPlaylist(Guid playlistId, Guid songId)
  {
    var userId = GetUserId();
    var success = await _playlistService.AddSongToPlaylistAsync(playlistId, songId, userId);

    if (!success)
      return NotFound();

    return NoContent();
  }

  [HttpDelete("{playlistId}/songs/{songId}")]
  public async Task<ActionResult> RemoveSongFromPlaylist(Guid playlistId, Guid songId)
  {
    var userId = GetUserId();
    var success = await _playlistService.RemoveSongFromPlaylistAsync(playlistId, songId, userId);

    if (!success)
      return NotFound();

    return NoContent();
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeletePlaylist(Guid id)
  {
    var userId = GetUserId();
    var success = await _playlistService.DeletePlaylistAsync(id, userId);

    if (!success)
      return NotFound();

    return NoContent();
  }

  private Guid GetUserId()
  {
    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("User ID not found in token");
    return Guid.Parse(userIdClaim);
  }
}

public class CreatePlaylistRequest
{
  public required string Name { get; set; }
  public string? Description { get; set; }
}