using WebApi.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Infrastructure.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly AppDbContext _context;

    public PlaylistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddPlaylistVideoAsync(string roomHash, PlaylistVideo playlistVideo)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var room = await _context.Rooms
                .Include(r => r.PlaylistVideos)
                .FirstOrDefaultAsync(r => r.Hash == roomHash);

            if (room == null)
            {
                return false;
            }

            room.PlaylistVideos.Add(playlistVideo);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PlaylistVideo> DeletePlaylistVideoAsync(string roomHash, string videoHash)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var room = await _context.Rooms
                .Include(r => r.PlaylistVideos)
                .FirstOrDefaultAsync(r => r.Hash == roomHash);

            if (room == null)
            {
                return null;
            }

            var playlistVideo = room.PlaylistVideos.FirstOrDefault(v => v.Hash == videoHash);

            if (playlistVideo == null)
            {
                return null;
            }

            room.PlaylistVideos.Remove(playlistVideo);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return playlistVideo;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
