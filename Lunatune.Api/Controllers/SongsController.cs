using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;
using System.Security.Claims;

namespace Lunatune.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SongsController(
    ISongService songService,
    IFileStorageService fileStorageService,
    ISongLikeService songLikeService,
    IUserService userService) : ControllerBase
{
    private readonly ISongService _songService = songService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly ISongLikeService _songLikeService = songLikeService;
    private readonly IUserService _userService = userService;

    /// Gets a paginated list of songs with optional search term
    [HttpGet]
    public async Task<ActionResult<object>> GetSongs([FromQuery] string? searchTerm = null, [FromQuery] int page = 1)
    {
        var userId = await GetCurrentUserId();
        var (songs, totalPages) = await _songService.GetSongsAsync(searchTerm, page, userId: userId);

        return Ok(new
        {
            songs,
            totalPages
        });
    }

    /// Gets a paginated list of popular songs sorted by number of likes
    [HttpGet("popular")]
    public async Task<ActionResult<object>> GetPopularSongs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = await GetCurrentUserId();
        var (songs, totalPages) = await _songService.GetSongsAsync(null, page, pageSize, userId);
        var sortedSongs = songs
            .Where(s => s.LikeCount > 0)
            .OrderByDescending(s => s.LikeCount)
            .ToList();

        return Ok(new
        {
            songs = sortedSongs,
            totalPages
        });
    }

    /// Gets a specific song by its ID
    [HttpGet("{id}")]
    public async Task<ActionResult<Song>> GetSong(Guid id)
    {
        var song = await _songService.GetSongByIdAsync(id);
        if (song == null)
        {
            return NotFound();
        }

        return Ok(song);
    }

    /// Gets a temporary streaming URL for a song
    [HttpGet("{id}/stream")]
    public async Task<IActionResult> GetStreamUrl(Guid id)
    {
        var song = await _songService.GetSongByIdAsync(id);
        if (song == null)
        {
            return NotFound();
        }

        try
        {
            var blobUrl = await _fileStorageService.GetBlobUrlAsync(song.FilePath);
            var sasToken = await _fileStorageService.GetSasTokenAsync(song.FilePath, TimeSpan.FromHours(1));
            var streamUrl = $"{blobUrl}?{sasToken}";

            return Ok(new { streamUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error generating stream URL: {ex.Message}");
        }
    }

    /// Likes a song for the current user
    [HttpPost("{id}/like")]
    public async Task<IActionResult> LikeSong(Guid id)
    {
        var userId = await GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var song = await _songService.GetSongByIdAsync(id);
        if (song == null)
        {
            return NotFound();
        }

        var success = await _songLikeService.LikeSongAsync(userId.Value, id);
        if (!success)
        {
            return BadRequest("Song is already liked");
        }

        return Ok();
    }

    /// Removes a like from a song for the current user
    [HttpDelete("{id}/like")]
    public async Task<IActionResult> UnlikeSong(Guid id)
    {
        var userId = await GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var song = await _songService.GetSongByIdAsync(id);
        if (song == null)
        {
            return NotFound();
        }

        var success = await _songLikeService.UnlikeSongAsync(userId.Value, id);
        if (!success)
        {
            return BadRequest("Song is not liked");
        }

        return Ok();
    }

    /// Checks if the current user has liked a specific song
    [HttpGet("{id}/like")]
    public async Task<IActionResult> IsSongLiked(Guid id)
    {
        var userId = await GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var song = await _songService.GetSongByIdAsync(id);
        if (song == null)
        {
            return NotFound();
        }

        var isLiked = await _songLikeService.IsSongLikedByUserAsync(userId.Value, id);
        return Ok(new { isLiked });
    }

    /// Gets the total number of likes for a specific song
    [HttpGet("{id}/likes")]
    public async Task<IActionResult> GetLikeCount(Guid id)
    {
        var song = await _songService.GetSongByIdAsync(id);
        if (song == null)
        {
            return NotFound();
        }

        var likeCount = await _songLikeService.GetSongLikeCountAsync(id);
        return Ok(new { likeCount });
    }

    /// Gets all songs that the current user has liked
    [HttpGet("liked")]
    public async Task<IActionResult> GetLikedSongs()
    {
        var userId = await GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var likedSongs = await _songLikeService.GetUserLikedSongsAsync(userId.Value);
        return Ok(likedSongs);
    }

    private async Task<Guid?> GetCurrentUserId()
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
