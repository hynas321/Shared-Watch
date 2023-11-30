public class PlaylistVideo
{
    public string Url { get; set; }
    public double Duration { get; set; }

    public PlaylistVideo(string url, double duration)
    {
        Url = url;
        Duration = duration;
    }
}