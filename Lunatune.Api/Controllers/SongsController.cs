using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;

namespace Lunatune.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SongsController(ISongService songService, IFileStorageService fileStorageService) : ControllerBase
{
    private readonly ISongService _songService = songService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<object>> GetSongs([FromQuery] string? searchTerm = null, [FromQuery] int page = 1)
    {
        var (songs, totalPages) = await _songService.GetSongsAsync(searchTerm, page);

        return Ok(new
        {
            songs,
            totalPages
        });
    }

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
}
