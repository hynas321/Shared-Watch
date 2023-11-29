public class VideoPlayerState {
    public PlaylistVideo PlaylistVideo { get; set; }
    public bool IsPlaying { get; set; }
    public double CurrentTime { get; set; }
    public double Duration { get; set; }

    public VideoPlayerState()
    {
        PlaylistVideo = new PlaylistVideo(null);
        IsPlaying = false;
        CurrentTime = 0;
    }
}