using DotnetServer.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetServer.Infrastructure.Repositories;

public class PlaylistRepository : IPlaylistRepository
{
    private readonly AppDbContext _context;

    public PlaylistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddPlaylistVideoAsync(string roomHash, PlaylistVideo playlistVideo)
    {
        Room room = await _context.Rooms
            .Include(r => r.PlaylistVideos)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        if (room == null)
        {
            return false;
        }

        room.PlaylistVideos.Add(playlistVideo);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PlaylistVideo> DeletePlaylistVideoAsync(string roomHash, string videoHash)
    {
        Room room = await _context.Rooms
            .Include(r => r.PlaylistVideos)
            .FirstOrDefaultAsync(r => r.Hash == roomHash);

        if (room == null)
        {
            return null;
        }

        PlaylistVideo playlistVideo = room.PlaylistVideos.FirstOrDefault(v => v.Hash == videoHash);

        if (playlistVideo == null)
        {
            return null;
        }

        room.PlaylistVideos.Remove(playlistVideo);
        await _context.SaveChangesAsync();

        return playlistVideo;
    }
}