using Lunatune.Core.Models;

namespace Lunatune.Core.Interfaces;

public interface ISongService
{
    Task<(IEnumerable<Song> Songs, int TotalPages)> GetSongsAsync(string? searchTerm = null, int page = 1, int pageSize = 10);
    Task<Song?> GetSongByIdAsync(Guid id);
}

