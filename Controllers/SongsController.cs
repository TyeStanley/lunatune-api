using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services;
using Models;
using System.Security.Claims;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SongsController(
    SongService songService,
    AzureBlobStorageService fileStorageService,
    SongLikeService songLikeService,
    UserService userService) : ControllerBase
{
    private readonly SongService _songService = songService;
    private readonly AzureBlobStorageService _fileStorageService = fileStorageService;
    private readonly SongLikeService _songLikeService = songLikeService;
    private readonly UserService _userService = userService;

    /// Gets a paginated list of songs with optional search term
    [HttpGet]
    public async Task<ActionResult<object>> GetSongs([FromQuery] string? searchTerm = null, [FromQuery] int page = 1, [FromQuery] string? sortBy = null)
    {
        var userId = await GetCurrentUserId();
        var (songs, totalPages) = await _songService.GetSongsAsync(searchTerm, page, userId: userId, sortBy: sortBy);

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
        var (songs, totalPages) = await _songService.GetSongsAsync(null, page, pageSize, userId, sortBy: "popular");

        return Ok(new
        {
            songs,
            totalPages
        });
    }

    /// Gets a specific song by its ID
    [HttpGet("{id}")]
    public async Task<ActionResult<SongWithLikeInfo>> GetSong(Guid id)
    {
        var userId = await GetCurrentUserId();
        var song = await _songService.GetSongByIdAsync(id, userId);
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
        var userId = await GetCurrentUserId();
        var song = await _songService.GetSongByIdAsync(id, userId);
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

        var song = await _songService.GetSongByIdAsync(id, userId);
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

        var song = await _songService.GetSongByIdAsync(id, userId);
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

        var song = await _songService.GetSongByIdAsync(id, userId);
        if (song == null)
        {
            return NotFound();
        }

        return Ok(new { isLiked = song.IsLiked });
    }

    /// Gets the total number of likes for a specific song
    [HttpGet("{id}/likes")]
    public async Task<IActionResult> GetLikeCount(Guid id)
    {
        var userId = await GetCurrentUserId();
        var song = await _songService.GetSongByIdAsync(id, userId);
        if (song == null)
        {
            return NotFound();
        }

        return Ok(new { likeCount = song.LikeCount });
    }

    /// Gets all songs that the current user has liked
    [HttpGet("liked")]
    public async Task<ActionResult<object>> GetLikedSongs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var userId = await GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var (songs, totalPages) = await _songService.GetSongsAsync(null, page, pageSize, userId, sortBy: "liked");
        return Ok(new
        {
            songs,
            totalPages
        });
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
