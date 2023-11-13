public class VideoPlayerSettings {
    public string Url { get; set; }
    public bool IsPlaying { get; set; }

    public VideoPlayerSettings()
    {
        Url = "https://www.youtube.com/watch?v=LXb3EKWsInQ";
        IsPlaying = false;
    }
}