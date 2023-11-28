public class VideoPlayerState {
    public PlaylistVideo PlaylistVideo { get; set; }
    public bool IsPlaying { get; set; }
    public double CurrentTime { get; set; }
    public double Duration { get; set; }

    public VideoPlayerState()
    {
        PlaylistVideo = new PlaylistVideo("https://www.youtube.com/watch?v=LXb3EKWsInQ");
        IsPlaying = false;
        CurrentTime = 0;
    }
}