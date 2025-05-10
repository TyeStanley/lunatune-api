using Lunatune.Core.Models;

namespace Lunatune.Core.Interfaces;

public interface ISongService
{
    Task<IEnumerable<Song>> GetAllSongsAsync();
    Task<Song?> GetSongByIdAsync(Guid id);
}

