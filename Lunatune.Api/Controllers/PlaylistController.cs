using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;
using System.Security.Claims;

namespace Lunatune.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaylistController(IPlaylistService playlistService) : ControllerBase
{
  private readonly IPlaylistService _playlistService = playlistService;

  [HttpGet]
  public async Task<ActionResult<IEnumerable<PlaylistWithUserInfo>>> GetUserPlaylists([FromQuery] string? searchTerm = null)
  {
    var userId = GetUserId();
    var playlists = await _playlistService.GetUserPlaylistsAsync(userId, searchTerm);
    return Ok(playlists);
  }

  [HttpGet("all")]
  public async Task<ActionResult<object>> GetAllPlaylists([FromQuery] string? searchTerm = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
  {
    var userId = GetUserId();
    var (playlists, totalPages) = await _playlistService.GetAllPlaylistsAsync(searchTerm, page, pageSize, userId);
    return Ok(new { playlists, totalPages });
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<PlaylistWithUserInfo>> GetPlaylist(Guid id)
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