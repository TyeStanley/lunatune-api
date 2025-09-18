using Lunatune.Models;
using Lunatune.Data;
using Microsoft.EntityFrameworkCore;

namespace Lunatune.Services;

public class SongLikeService(ApplicationDbContext context, PlaylistService playlistService)
{
  private readonly ApplicationDbContext _context = context;
  private readonly PlaylistService _playlistService = playlistService;

  public async Task<bool> LikeSongAsync(Guid userId, Guid songId)
  {
    var existingLike = await _context.SongLikes
        .FirstOrDefaultAsync(l => l.UserId == userId && l.SongId == songId);

    if (existingLike != null)
      return false;

    var like = new SongLike
    {
      UserId = userId,
      SongId = songId
    };

    _context.SongLikes.Add(like);
    await _context.SaveChangesAsync();

    // Add to Liked Songs playlist
    var likedSongsPlaylist = await _playlistService.GetOrCreateLikedSongsPlaylistAsync(userId);
    await _playlistService.AddSongToPlaylistAsync(likedSongsPlaylist.Id, songId, userId);

    return true;
  }

  public async Task<bool> UnlikeSongAsync(Guid userId, Guid songId)
  {
    var like = await _context.SongLikes
        .FirstOrDefaultAsync(l => l.UserId == userId && l.SongId == songId);

    if (like == null)
      return false;

    _context.SongLikes.Remove(like);
    await _context.SaveChangesAsync();

    // Remove from Liked Songs playlist
    var likedSongsPlaylist = await _playlistService.GetOrCreateLikedSongsPlaylistAsync(userId);
    await _playlistService.RemoveSongFromPlaylistAsync(likedSongsPlaylist.Id, songId, userId);

    return true;
  }

  public async Task<bool> IsSongLikedByUserAsync(Guid userId, Guid songId)
  {
    return await _context.SongLikes
        .AnyAsync(l => l.UserId == userId && l.SongId == songId);
  }

  public async Task<int> GetSongLikeCountAsync(Guid songId)
  {
    return await _context.SongLikes
        .CountAsync(l => l.SongId == songId);
  }
}