namespace WebApi.Core.Entities.In_memory;

public class VideoPlayer
{
    public PlaylistVideo PlaylistVideo { get; set; }
    public bool IsPlaying { get; set; }
    public double CurrentTime { get; set; }
    public bool IsCurrentVideoRemoved { get; set; }

    public VideoPlayer()
    {
        PlaylistVideo = new PlaylistVideo();
        IsPlaying = false;
        CurrentTime = 0;
        IsCurrentVideoRemoved = false;
    }
}