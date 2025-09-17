using Models;
using Data;
using Microsoft.EntityFrameworkCore;

namespace Services;

public class SongService(ApplicationDbContext context)
{
    private readonly ApplicationDbContext _context = context;

    public async Task<(IEnumerable<SongWithLikeInfo> Songs, int TotalPages)> GetSongsAsync(string? searchTerm = null, int page = 1, int pageSize = 10, Guid? userId = null, string? sortBy = null)
    {
        var query = _context.Songs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(s =>
                EF.Functions.ILike(s.Title, $"%{searchTerm}%") ||
                EF.Functions.ILike(s.Artist, $"%{searchTerm}%"));
        }

        if (sortBy?.ToLower() == "popular")
        {
            query = query.Where(s => s.Likes.Any())
                        .OrderByDescending(s => s.Likes.Count);
        }
        else if (sortBy?.ToLower() == "liked" && userId.HasValue)
        {
            query = query.Where(s => s.Likes.Any(l => l.UserId == userId.Value))
                        .OrderBy(s => s.Title);
        }
        else
        {
            query = query.OrderBy(s => s.Title);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var songs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SongWithLikeInfo
            {
                Id = s.Id,
                Title = s.Title,
                Artist = s.Artist,
                Album = s.Album,
                Genre = s.Genre,
                FilePath = s.FilePath,
                DurationMs = s.DurationMs,
                AlbumArtUrl = s.AlbumArtUrl,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                IsLiked = userId.HasValue && s.Likes.Any(l => l.UserId == userId.Value),
                LikeCount = s.Likes.Count
            })
            .ToListAsync();

        return (songs, totalPages);
    }

    public async Task<SongWithLikeInfo?> GetSongByIdAsync(Guid id, Guid? userId = null)
    {
        var song = await _context.Songs
            .Include(s => s.Likes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (song == null)
            return null;

        return new SongWithLikeInfo
        {
            Id = song.Id,
            Title = song.Title,
            Artist = song.Artist,
            Album = song.Album,
            Genre = song.Genre,
            FilePath = song.FilePath,
            DurationMs = song.DurationMs,
            AlbumArtUrl = song.AlbumArtUrl,
            CreatedAt = song.CreatedAt,
            UpdatedAt = song.UpdatedAt,
            IsLiked = userId.HasValue && song.Likes.Any(l => l.UserId == userId.Value),
            LikeCount = song.Likes.Count
        };
    }
}
