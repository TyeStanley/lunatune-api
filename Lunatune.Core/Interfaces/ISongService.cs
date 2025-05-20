using Lunatune.Core.Models;

namespace Lunatune.Core.Interfaces;

public interface ISongService
{
    Task<(IEnumerable<SongWithLikeInfo> Songs, int TotalPages)> GetSongsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, Guid? userId = null, string? sortBy = null);
    Task<SongWithLikeInfo?> GetSongByIdAsync(Guid id, Guid? userId = null);
}

