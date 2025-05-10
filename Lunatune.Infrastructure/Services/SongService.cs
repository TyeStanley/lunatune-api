using Lunatune.Core.Interfaces;
using Lunatune.Core.Models;
using Lunatune.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lunatune.Infrastructure.Services;

public class SongService(ApplicationDbContext context) : ISongService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<(IEnumerable<Song> Songs, int TotalPages)> GetSongsAsync(string? searchTerm = null, int page = 1, int pageSize = 10)
    {
        var query = _context.Songs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s =>
                s.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                s.Artist.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var songs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (songs, totalPages);
    }

    public async Task<Song?> GetSongByIdAsync(Guid id)
    {
        return await _context.Songs.FindAsync(id);
    }
}
