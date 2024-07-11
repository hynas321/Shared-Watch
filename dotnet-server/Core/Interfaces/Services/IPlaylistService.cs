public interface IPlaylistService
{
    public bool IsServiceRunning { get; }
    public void StartPlaylistService(string roomHash);
}