using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;

namespace Lunatune.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SongsController(IMusicService musicService, IFileStorageService fileStorageService) : ControllerBase
{
    private readonly IMusicService _musicService = musicService;
    private readonly IFileStorageService _fileStorageService = fileStorageService;

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
    {
        var songs = await _musicService.GetAllSongsAsync();
        return Ok(songs);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Song>> GetSong(Guid id)
    {
        var song = await _musicService.GetSongByIdAsync(id);
        if (song == null)
        {
            return NotFound();
        }

        return Ok(song);
    }

    [HttpGet("{id}/stream")]
    public async Task<IActionResult> GetStreamUrl(Guid id)
    {
        var song = await _musicService.GetSongByIdAsync(id);
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
