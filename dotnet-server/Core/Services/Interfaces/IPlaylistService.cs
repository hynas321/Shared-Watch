
namespace DotnetServer.Core.Services;

public interface IPlaylistService
{
    bool IsServiceRunning { get; }
    void StartPlaylistService(string roomHash);
}