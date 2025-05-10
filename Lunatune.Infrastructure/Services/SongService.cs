using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;
using Lunatune.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lunatune.Infrastructure.Services;

public class SongService(ApplicationDbContext context) : ISongService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Song>> GetAllSongsAsync()
    {
        return await _context.Songs.ToListAsync();
    }

    public async Task<Song?> GetSongByIdAsync(Guid id)
    {
        return await _context.Songs.FindAsync(id);
    }
}
