using Lunatune.Core.Models;

namespace Lunatune.Core.Interfaces;

public interface IPlaylistService
{
  Task<Playlist> GetOrCreateLikedSongsPlaylistAsync(Guid userId);
  Task<Playlist> CreatePlaylistAsync(Guid userId, string name, string? description = null, bool isPublic = false);
  Task<PlaylistWithSongDto?> GetPlaylistByIdAsync(Guid playlistId, Guid? userId = null);
  Task<IEnumerable<PlaylistWithUserInfo>> GetUserPlaylistsAsync(Guid userId, string? searchTerm = null);
  Task<(IEnumerable<PlaylistWithUserInfo> Playlists, int TotalPages)> GetAllPlaylistsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, Guid? userId = null);
  Task<AddSongToPlaylistResult> AddSongToPlaylistAsync(Guid playlistId, Guid songId, Guid userId);
  Task<bool> RemoveSongFromPlaylistAsync(Guid playlistId, Guid songId, Guid userId);
  Task<bool> DeletePlaylistAsync(Guid playlistId, Guid userId);
  Task<bool> AddPlaylistToLibraryAsync(Guid playlistId, Guid userId);
  Task<bool> RemovePlaylistFromLibraryAsync(Guid playlistId, Guid userId);
  Task<Playlist?> EditPlaylistAsync(Guid playlistId, string? name, string? description, bool? isPublic, Guid userId);
}