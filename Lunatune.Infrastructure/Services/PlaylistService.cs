using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;
using Lunatune.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lunatune.Infrastructure.Services;

public class PlaylistService(ApplicationDbContext context) : IPlaylistService
{
  private readonly ApplicationDbContext _context = context;
  private const string LIKED_SONGS_PLAYLIST_NAME = "Liked Songs";

  // Get or create the Liked Songs playlist
  public async Task<Playlist> GetOrCreateLikedSongsPlaylistAsync(Guid userId)
  {
    var likedSongsPlaylist = await _context.Playlists
        .FirstOrDefaultAsync(p => p.CreatorId == userId && p.Name == LIKED_SONGS_PLAYLIST_NAME);

    if (likedSongsPlaylist != null)
      return likedSongsPlaylist;

    // Create new Liked Songs playlist
    likedSongsPlaylist = new Playlist
    {
      Name = LIKED_SONGS_PLAYLIST_NAME,
      Description = "Your liked songs",
      CreatorId = userId
    };

    _context.Playlists.Add(likedSongsPlaylist);
    await _context.SaveChangesAsync();

    return likedSongsPlaylist;
  }

  // Create a new playlist
  public async Task<Playlist> CreatePlaylistAsync(Guid userId, string name, string? description = null)
  {
    var playlist = new Playlist
    {
      Name = name,
      Description = description,
      CreatorId = userId
    };

    _context.Playlists.Add(playlist);
    await _context.SaveChangesAsync();

    return playlist;
  }

  // Get a playlist by ID
  public async Task<Playlist?> GetPlaylistByIdAsync(Guid playlistId)
  {
    return await _context.Playlists
        .Include(p => p.Songs)
            .ThenInclude(ps => ps.Song)
        .Include(p => p.Creator)
        .FirstOrDefaultAsync(p => p.Id == playlistId);
  }

  // Get all playlists for a user (without songs)
  public async Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(Guid userId)
  {
    return await _context.Playlists
        .Include(p => p.Creator)
        .Where(p => p.CreatorId == userId || p.LibraryEntries.Any(le => le.UserId == userId))
        .OrderBy(p => p.Name)
        .ToListAsync();
  }

  // Add a song to a playlist if you are the creator
  public async Task<bool> AddSongToPlaylistAsync(Guid playlistId, Guid songId, Guid userId)
  {
    var playlist = await _context.Playlists
        .FirstOrDefaultAsync(p => p.Id == playlistId && p.CreatorId == userId);

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

  // Remove a song from a playlist if you are the creator
  public async Task<bool> RemoveSongFromPlaylistAsync(Guid playlistId, Guid songId, Guid userId)
  {
    var playlistSong = await _context.PlaylistSongs
        .Include(ps => ps.Playlist)
        .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId && ps.Playlist.CreatorId == userId);

    if (playlistSong == null)
      return false;

    _context.PlaylistSongs.Remove(playlistSong);
    await _context.SaveChangesAsync();

    return true;
  }

  // Delete a playlist if you are the creator
  public async Task<bool> DeletePlaylistAsync(Guid playlistId, Guid userId)
  {
    var playlist = await _context.Playlists
        .FirstOrDefaultAsync(p => p.Id == playlistId && p.CreatorId == userId);

    if (playlist == null)
      return false;

    _context.Playlists.Remove(playlist);
    await _context.SaveChangesAsync();

    return true;
  }

  // Add a playlist to user's library
  public async Task<bool> AddPlaylistToLibraryAsync(Guid playlistId, Guid userId)
  {
    // Check if playlist exists
    var playlist = await _context.Playlists
        .FirstOrDefaultAsync(p => p.Id == playlistId);

    if (playlist == null)
      return false;

    // Check if already in library
    var exists = await _context.UserLibraryPlaylists
        .AnyAsync(ul => ul.PlaylistId == playlistId && ul.UserId == userId);

    if (exists)
      return true; // Already in library, not an error

    var libraryEntry = new UserLibraryPlaylist
    {
      PlaylistId = playlistId,
      UserId = userId
    };

    _context.UserLibraryPlaylists.Add(libraryEntry);
    await _context.SaveChangesAsync();

    return true;
  }

  // Remove a playlist from user's library
  public async Task<bool> RemovePlaylistFromLibraryAsync(Guid playlistId, Guid userId)
  {
    var libraryEntry = await _context.UserLibraryPlaylists
        .FirstOrDefaultAsync(ul => ul.PlaylistId == playlistId && ul.UserId == userId);

    if (libraryEntry == null)
      return false;

    _context.UserLibraryPlaylists.Remove(libraryEntry);
    await _context.SaveChangesAsync();

    return true;
  }
}