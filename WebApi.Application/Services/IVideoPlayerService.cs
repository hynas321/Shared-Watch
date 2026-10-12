namespace WebApi.Application.Services;

public interface IVideoPlayerService
{
    bool IsServiceRunning { get; }
    Task<WebApi.Core.Entities.PlaylistVideo?> RemovePlaylistVideoAsync(string roomHash, string videoHash, CancellationToken cancellationToken);
    void StartPlaylistService(string roomHash);
}
