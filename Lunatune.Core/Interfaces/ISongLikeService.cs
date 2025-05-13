using Lunatune.Core.Models;

namespace Lunatune.Core.Interfaces;

public interface ISongLikeService
{
  Task<bool> LikeSongAsync(Guid userId, Guid songId);
  Task<bool> UnlikeSongAsync(Guid userId, Guid songId);
  Task<bool> IsSongLikedByUserAsync(Guid userId, Guid songId);
  Task<int> GetSongLikeCountAsync(Guid songId);
  Task<IEnumerable<Song>> GetUserLikedSongsAsync(Guid userId);
}