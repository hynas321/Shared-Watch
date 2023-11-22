public class VideoPlayerState {
    public string Url { get; set; }
    public bool IsPlaying { get; set; }

    public VideoPlayerState()
    {
        Url = "https://www.youtube.com/watch?v=LXb3EKWsInQ";
        IsPlaying = false;
    }
}