using WebApi.Infrastructure.Persistence;
using WebApi.Core.Entities;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Repositories;

namespace WebApi.Infrastructure.Persistence.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly AppDbContext _dbContext;

    public PlaylistRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PlaylistVideoAddResult> AddPlaylistVideoAsync(string roomHash, PlaylistVideo playlistVideo, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.PlaylistVideos)
            .FirstOrDefaultAsync(r => r.Hash == roomHash, cancellationToken);

        if (room is null)
        {
            return PlaylistVideoAddResult.RoomNotFound;
        }

        if (room.PlaylistVideos.Count >= 10)
        {
            return PlaylistVideoAddResult.PlaylistFull;
        }

        playlistVideo.Position = room.PlaylistVideos.Count;
        room.PlaylistVideos?.Add(playlistVideo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return PlaylistVideoAddResult.Added;
    }

    public async Task<PlaylistVideo?> DeletePlaylistVideoAsync(string roomHash, string videoHash, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.PlaylistVideos)
            .FirstOrDefaultAsync(r => r.Hash == roomHash, cancellationToken);

        if (room is null)
        {
            return null;
        }

        var playlistVideo = room?.PlaylistVideos?.FirstOrDefault(v => v.Hash == videoHash);

        if (playlistVideo is null)
        {
            return null;
        }

        _dbContext.PlaylistVideos.Remove(playlistVideo);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return playlistVideo;
    }

    public async Task<PlaylistVideo?> GetPlaylistVideoAsync(string roomHash, string videoHash, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Include(r => r.PlaylistVideos)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        if (room is null)
        {
            return null;
        }

        var playlistVideo = room.PlaylistVideos?.FirstOrDefault(v => v.Hash == videoHash);

        return playlistVideo;
    }

    public async Task<bool> RoomHasPlaylistVideoUrlAsync(string roomHash, string url, CancellationToken cancellationToken)
    {
        return await _dbContext.PlaylistVideos.AnyAsync(
            video => video.RoomHash == roomHash && video.Url == url,
            cancellationToken);
    }

    public async Task<bool> ReorderPlaylistVideosAsync(string roomHash, IReadOnlyList<string> videoHashes, CancellationToken cancellationToken)
    {
        var room = await _dbContext.Rooms
            .Include(currentRoom => currentRoom.PlaylistVideos)
            .FirstOrDefaultAsync(currentRoom => currentRoom.Hash == roomHash, cancellationToken);

        if (room is null || room.PlaylistVideos.Count != videoHashes.Count ||
            room.PlaylistVideos.Select(video => video.Hash).ToHashSet().Count != videoHashes.Count ||
            videoHashes.Any(hash => !room.PlaylistVideos.Any(video => video.Hash == hash)))
        {
            return false;
        }

        for (var position = 0; position < videoHashes.Count; position++)
        {
            room.PlaylistVideos.Single(video => video.Hash == videoHashes[position]).Position = position;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}


