using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;
using Lunatune.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lunatune.Infrastructure.Services;

public class SongLikeService(ApplicationDbContext context) : ISongLikeService
{
  private readonly ApplicationDbContext _context = context;

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

  public async Task<IEnumerable<Song>> GetUserLikedSongsAsync(Guid userId)
  {
    return await _context.SongLikes
        .Where(l => l.UserId == userId)
        .Include(l => l.Song)
        .Select(l => l.Song)
        .ToListAsync();
  }
}