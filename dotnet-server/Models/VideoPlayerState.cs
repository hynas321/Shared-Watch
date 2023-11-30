public class VideoPlayerState {
    public PlaylistVideo PlaylistVideo { get; set; }
    public bool IsPlaying { get; set; }
    public double CurrentTime { get; set; }

    public VideoPlayerState()
    {
        PlaylistVideo = new PlaylistVideo(null, 3);
        IsPlaying = false;
        CurrentTime = 0;
    }
}