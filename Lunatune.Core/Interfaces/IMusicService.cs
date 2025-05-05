using Lunatune.Core.Models;

namespace Lunatune.Core.Interfaces;

public interface IMusicService
{
    Task<IEnumerable<Song>> GetAllSongsAsync();
    Task<Song?> GetSongByIdAsync(Guid id);
}

