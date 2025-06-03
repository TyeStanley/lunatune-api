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
  public async Task<Playlist> CreatePlaylistAsync(Guid userId, string name, string? description = null, bool isPublic = false)
  {
    var playlist = new Playlist
    {
      Name = name,
      Description = description,
      CreatorId = userId,
      IsPublic = isPublic
    };

    _context.Playlists.Add(playlist);
    await _context.SaveChangesAsync();

    var libraryEntry = new UserLibraryPlaylist
    {
      PlaylistId = playlist.Id,
      UserId = userId
    };

    _context.UserLibraryPlaylists.Add(libraryEntry);
    await _context.SaveChangesAsync();

    return playlist;
  }

  // Get a playlist by ID
  public async Task<PlaylistWithSongDto?> GetPlaylistByIdAsync(Guid playlistId, Guid? userId = null)
  {
    var playlist = await _context.Playlists
        .Include(p => p.Songs)
            .ThenInclude(ps => ps.Song)
        .Include(p => p.Creator)
        .FirstOrDefaultAsync(p => p.Id == playlistId &&
            (p.IsPublic || (userId.HasValue && p.CreatorId == userId.Value)));

    if (playlist == null)
      return null;

    return new PlaylistWithSongDto
    {
      Id = playlist.Id,
      Name = playlist.Name,
      Description = playlist.Description,
      CreatorId = playlist.CreatorId,
      CreatedAt = playlist.CreatedAt,
      UpdatedAt = playlist.UpdatedAt ?? playlist.CreatedAt,
      IsCreator = userId.HasValue && playlist.CreatorId == userId.Value,
      Creator = new CreatorInfoDto { Name = playlist.Creator.Name },
      Songs = playlist.Songs.Select(ps => ps.Song).ToList()
    };
  }

  // Get all playlists for a user (without songs)
  public async Task<IEnumerable<PlaylistWithUserInfo>> GetUserPlaylistsAsync(Guid userId, string? searchTerm = null)
  {
    // Determine if Liked Songs should be included
    bool includeLikedSongs = string.IsNullOrWhiteSpace(searchTerm) ||
        LIKED_SONGS_PLAYLIST_NAME.Contains(searchTerm!.Trim(), StringComparison.OrdinalIgnoreCase) ||
        searchTerm.Trim().Contains(LIKED_SONGS_PLAYLIST_NAME, StringComparison.OrdinalIgnoreCase);

    PlaylistWithUserInfo? likedSongsDto = null;
    if (includeLikedSongs)
    {
      var likedSongsPlaylist = await GetOrCreateLikedSongsPlaylistAsync(userId);
      var user = await _context.Users.AsNoTracking().FirstAsync(u => u.Id == userId);
      likedSongsDto = new PlaylistWithUserInfo
      {
        Id = likedSongsPlaylist.Id,
        Name = likedSongsPlaylist.Name,
        Description = likedSongsPlaylist.Description,
        CreatorId = likedSongsPlaylist.CreatorId,
        CreatedAt = likedSongsPlaylist.CreatedAt,
        UpdatedAt = likedSongsPlaylist.UpdatedAt,
        IsCreator = true,
        IsPublic = likedSongsPlaylist.IsPublic,
        Creator = new CreatorInfoDto { Name = user.Name }
      };
    }

    // Get playlists from user's library and created playlists
    var query = _context.Playlists
        .Include(p => p.Creator)
        .Where(p => p.CreatorId == userId || // User's created playlists
                    _context.UserLibraryPlaylists.Any(ul => // User's library playlists
                        ul.UserId == userId &&
                        ul.PlaylistId == p.Id))
        .Where(p => p.Name != LIKED_SONGS_PLAYLIST_NAME);

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
      query = query.Where(p =>
          EF.Functions.ILike(p.Name, $"%{searchTerm}%"));
    }

    var playlists = await query
        .OrderBy(p => p.Name)
        .Select(p => new PlaylistWithUserInfo
        {
          Id = p.Id,
          Name = p.Name,
          Description = p.Description,
          CreatorId = p.CreatorId,
          CreatedAt = p.CreatedAt,
          UpdatedAt = p.UpdatedAt,
          IsCreator = p.CreatorId == userId,
          IsInLibrary = p.CreatorId != userId, // If not creator, it must be in library
          IsPublic = p.IsPublic,
          Creator = new CreatorInfoDto { Name = p.Creator.Name }
        })
        .ToListAsync();

    // Prepend the Liked Songs playlist if it should be included
    if (likedSongsDto != null)
      playlists.Insert(0, likedSongsDto);

    return playlists;
  }

  // Get all playlists with pagination and search
  public async Task<(IEnumerable<PlaylistWithUserInfo> Playlists, int TotalPages)> GetAllPlaylistsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, Guid? userId = null)
  {
    var query = _context.Playlists
        .Include(p => p.Creator)
        .Where(p => p.Name != LIKED_SONGS_PLAYLIST_NAME)
        .Where(p => p.IsPublic || (userId.HasValue && p.CreatorId == userId.Value))
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
      query = query.Where(p =>
          EF.Functions.ILike(p.Name, $"%{searchTerm}%"));
    }

    var totalCount = await query.CountAsync();
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

    var playlists = await query
        .OrderBy(p => p.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(p => new PlaylistWithUserInfo
        {
          Id = p.Id,
          Name = p.Name,
          Description = p.Description,
          CreatorId = p.CreatorId,
          CreatedAt = p.CreatedAt,
          UpdatedAt = p.UpdatedAt,
          IsCreator = userId.HasValue && p.CreatorId == userId.Value,
          IsInLibrary = userId.HasValue && _context.UserLibraryPlaylists
            .Any(ul => ul.UserId == userId.Value && ul.PlaylistId == p.Id),
          IsPublic = p.IsPublic,
          Creator = new CreatorInfoDto { Name = p.Creator.Name }
        })
        .ToListAsync();

    return (playlists, totalPages);
  }

  // Add a song to a playlist if you are the creator or have it in your library
  public async Task<AddSongToPlaylistResult> AddSongToPlaylistAsync(Guid playlistId, Guid songId, Guid userId)
  {
    var playlist = await _context.Playlists
        .FirstOrDefaultAsync(p => p.Id == playlistId && (p.CreatorId == userId || p.LibraryEntries.Any(le => le.UserId == userId)));

    if (playlist == null)
    {
      return AddSongToPlaylistResult.PlaylistNotFound;
    }

    var exists = await _context.PlaylistSongs
        .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);


    if (exists)
    {
      return AddSongToPlaylistResult.AlreadyExists;
    }

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

    return AddSongToPlaylistResult.Success;
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

    // Delete all library entries for this playlist
    var libraryEntries = await _context.UserLibraryPlaylists
        .Where(ul => ul.PlaylistId == playlistId)
        .ToListAsync();

    _context.UserLibraryPlaylists.RemoveRange(libraryEntries);
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

  // Edit a playlist
  public async Task<Playlist?> EditPlaylistAsync(Guid playlistId, string? name, string? description, bool? isPublic, Guid userId)
  {
    var playlist = await _context.Playlists
        .FirstOrDefaultAsync(p => p.Id == playlistId && p.CreatorId == userId);

    if (playlist == null)
      return null;

    if (name != null && name != playlist.Name)
      playlist.Name = name;

    if (description != null && description != playlist.Description)
      playlist.Description = description;

    if (isPublic.HasValue && isPublic.Value != playlist.IsPublic)
      playlist.IsPublic = isPublic.Value;

    await _context.SaveChangesAsync();
    return playlist;
  }
}
