public interface IPlaylistService
{
    bool IsHandlerRunning { get; set; }
    public void StartPlaylistService(string roomHash);
    public Task ManagePlaylistService(IRoom room);
}