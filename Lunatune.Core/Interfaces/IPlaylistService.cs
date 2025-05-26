using Lunatune.Core.Models;

namespace Lunatune.Core.Interfaces;

public interface IPlaylistService
{
  Task<Playlist> GetOrCreateLikedSongsPlaylistAsync(Guid userId);
  Task<Playlist> CreatePlaylistAsync(Guid userId, string name, string? description = null);
  Task<Playlist?> GetPlaylistByIdAsync(Guid playlistId, Guid userId);
  Task<IEnumerable<Playlist>> GetUserPlaylistsAsync(Guid userId);
  Task<bool> AddSongToPlaylistAsync(Guid playlistId, Guid songId, Guid userId);
  Task<bool> RemoveSongFromPlaylistAsync(Guid playlistId, Guid songId, Guid userId);
  Task<bool> DeletePlaylistAsync(Guid playlistId, Guid userId);
}