using WebApi.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Infrastructure.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly AppDbContext _dbContext;

    public PlaylistRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AddPlaylistVideoAsync(string roomHash, PlaylistVideo playlistVideo, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.PlaylistVideos)
            .FirstOrDefaultAsync(r => r.Hash == roomHash, cancellationToken);

        if (room is null)
        {
            return false;
        }

        room.PlaylistVideos.Add(playlistVideo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<PlaylistVideo> DeletePlaylistVideoAsync(string roomHash, string videoHash, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.PlaylistVideos)
            .FirstOrDefaultAsync(r => r.Hash == roomHash, cancellationToken);

        if (room is null)
        {
            return null;
        }

        var playlistVideo = room.PlaylistVideos.FirstOrDefault(v => v.Hash == videoHash);

        if (playlistVideo is null)
        {
            return null;
        }

        _dbContext.PlaylistVideos.Remove(playlistVideo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return playlistVideo;
    }

    public async Task<PlaylistVideo> GetPlaylistVideoAsync(string roomHash, string videoHash, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.PlaylistVideos)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        if (room is null)
        {
            return null;
        }

        var playlistVideo = room.PlaylistVideos.FirstOrDefault(v => v.Hash == videoHash);

        return playlistVideo;
    }
}
