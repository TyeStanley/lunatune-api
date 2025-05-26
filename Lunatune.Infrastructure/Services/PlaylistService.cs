using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;
using Lunatune.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lunatune.Infrastructure.Services;

public class PlaylistService(ApplicationDbContext context) : IPlaylistService
{
  private readonly ApplicationDbContext _context = context;
  private const string LIKED_SONGS_PLAYLIST_NAME = "Liked Songs";

  public async Task<Playlist> GetOrCreateLikedSongsPlaylistAsync(Guid userId)
  {
    var likedSongsPlaylist = await _context.Playlists
        .FirstOrDefaultAsync(p => p.UserId == userId && p.Name == LIKED_SONGS_PLAYLIST_NAME);

    if (likedSongsPlaylist != null)
      return likedSongsPlaylist;

    // Create new Liked Songs playlist
    likedSongsPlaylist = new Playlist
    {
      Name = LIKED_SONGS_PLAYLIST_NAME,
      Description = "Your liked songs",
      UserId = userId
    };

    _context.Playlists.Add(likedSongsPlaylist);
    await _context.SaveChangesAsync();

    return likedSongsPlaylist;
  }

  public async Task<Playlist> CreatePlaylistAsync(Guid userId, string name, string? description = null)
  {
    var playlist = new Playlist
    {
      Name = name,
      Description = description,
      UserId = userId
    };

    _context.Playlists.Add(playlist);
    await _context.SaveChangesAsync();

    return playlist;
  }

  public async Task<Playlist?> GetPlaylistByIdAsync(Guid playlistId, Guid userId)
  {
    return await _context.Playlists
        .Include(p => p.Songs)
            .ThenInclude(ps => ps.Song)
        .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);
  }

  public async Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(Guid userId)
  {
    return await _context.Playlists
        .Include(p => p.Songs)
            .ThenInclude(ps => ps.Song)
        .Where(p => p.UserId == userId)
        .OrderBy(p => p.Name)
        .ToListAsync();
  }

  public async Task<bool> AddSongToPlaylistAsync(Guid playlistId, Guid songId, Guid userId)
  {
    var playlist = await _context.Playlists
        .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

    if (playlist == null)
      return false;

    // Check if song already exists in playlist
    var exists = await _context.PlaylistSongs
        .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);

    if (exists)
      return true;

    // Get the highest position in the playlist
    var maxPosition = await _context.PlaylistSongs
        .Where(ps => ps.PlaylistId == playlistId)
        .MaxAsync(ps => (int?)ps.Position) ?? -1;

    var playlistSong = new PlaylistSong
    {
      PlaylistId = playlistId,
      SongId = songId,
      Position = maxPosition + 1
    };

    _context.PlaylistSongs.Add(playlistSong);
    await _context.SaveChangesAsync();

    return true;
  }

  public async Task<bool> RemoveSongFromPlaylistAsync(Guid playlistId, Guid songId, Guid userId)
  {
    var playlistSong = await _context.PlaylistSongs
        .Include(ps => ps.Playlist)
        .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId && ps.Playlist.UserId == userId);

    if (playlistSong == null)
      return false;

    _context.PlaylistSongs.Remove(playlistSong);
    await _context.SaveChangesAsync();

    return true;
  }

  public async Task<bool> DeletePlaylistAsync(Guid playlistId, Guid userId)
  {
    var playlist = await _context.Playlists
        .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

    if (playlist == null)
      return false;

    _context.Playlists.Remove(playlist);
    await _context.SaveChangesAsync();

    return true;
  }
}