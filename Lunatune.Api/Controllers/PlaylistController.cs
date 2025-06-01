using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;
using System.Security.Claims;

namespace Lunatune.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlaylistController(IPlaylistService playlistService, IUserService userService) : ControllerBase
{
  private readonly IPlaylistService _playlistService = playlistService;
  private readonly IUserService _userService = userService;

  // Gets user playlists has filtering
  [HttpGet]
  public async Task<ActionResult<IEnumerable<PlaylistWithUserInfo>>> GetUserPlaylists([FromQuery] string? searchTerm = null)
  {
    var userId = await GetUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var playlists = await _playlistService.GetUserPlaylistsAsync(userId.Value, searchTerm);
    return Ok(playlists);
  }

  // Get all playlists with pagination and search
  [HttpGet("all")]
  public async Task<ActionResult<object>> GetAllPlaylists([FromQuery] string? searchTerm = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
  {
    var userId = await GetUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var (playlists, totalPages) = await _playlistService.GetAllPlaylistsAsync(searchTerm, page, pageSize, userId.Value);
    return Ok(new { playlists, totalPages });
  }

  // Get a playlist by ID
  [HttpGet("{id}")]
  public async Task<ActionResult<PlaylistWithUserInfo>> GetPlaylist(Guid id)
  {
    var userId = await GetUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var playlist = await _playlistService.GetPlaylistByIdAsync(id, userId.Value);

    if (playlist == null)
      return NotFound();

    return Ok(playlist);
  }

  // Create a new playlist
  [HttpPost]
  public async Task<ActionResult<Playlist>> CreatePlaylist([FromBody] CreatePlaylistRequest request)
  {
    var userId = await GetUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var playlist = await _playlistService.CreatePlaylistAsync(userId.Value, request.Name, request.Description);
    return CreatedAtAction(nameof(GetPlaylist), new { id = playlist.Id }, playlist);
  }

  // Add a song to a playlist
  [HttpPost("{playlistId}/songs/{songId}")]
  public async Task<ActionResult> AddSongToPlaylist(Guid playlistId, Guid songId)
  {
    var userId = await GetUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var success = await _playlistService.AddSongToPlaylistAsync(playlistId, songId, userId.Value);

    if (!success)
      return NotFound();

    return NoContent();
  }

  // Remove a song from a playlist
  [HttpDelete("{playlistId}/songs/{songId}")]
  public async Task<ActionResult> RemoveSongFromPlaylist(Guid playlistId, Guid songId)
  {
    var userId = await GetUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var success = await _playlistService.RemoveSongFromPlaylistAsync(playlistId, songId, userId.Value);

    if (!success)
      return NotFound();

    return NoContent();
  }

  // Delete a playlist
  [HttpDelete("{id}")]
  public async Task<ActionResult> DeletePlaylist(Guid id)
  {
    var userId = await GetUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var success = await _playlistService.DeletePlaylistAsync(id, userId.Value);

    if (!success)
      return NotFound();

    return NoContent();
  }

  // Add a playlist to the library
  [HttpPost("{playlistId}/library")]
  public async Task<ActionResult> AddPlaylistToLibrary(Guid playlistId)
  {
    var userId = await GetUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var success = await _playlistService.AddPlaylistToLibraryAsync(playlistId, userId.Value);
    if (!success)
      return NotFound();
    return NoContent();
  }

  // Remove a playlist from the library
  [HttpDelete("{playlistId}/library")]
  public async Task<ActionResult> RemovePlaylistFromLibrary(Guid playlistId)
  {
    var userId = await GetUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var success = await _playlistService.RemovePlaylistFromLibraryAsync(playlistId, userId.Value);
    if (!success)
      return NotFound();
    return NoContent();
  }

  // Get or create the Liked Songs playlist
  [HttpGet("liked")]
  public async Task<ActionResult<Playlist>> GetOrCreateLikedSongsPlaylist()
  {
    var userId = await GetUserId();
    if (userId == null)
    {
      return Unauthorized();
    }

    var playlist = await _playlistService.GetOrCreateLikedSongsPlaylistAsync(userId.Value);

    return Ok(playlist);
  }

  private async Task<Guid?> GetUserId()
  {
    var auth0Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(auth0Id))
    {
      return null;
    }

    var user = await _userService.GetUserByAuth0IdAsync(auth0Id);
    return user?.Id;
  }
}

public class CreatePlaylistRequest
{
  public required string Name { get; set; }
  public string? Description { get; set; }
}