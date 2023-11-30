using System.Runtime.CompilerServices;

public class PlaylistVideo
{
    public string Hash { get; set; }
    public string Url { get; set; }
    public double Duration { get; set; }
    public string Title { get; set; }
    public string ThumbnailUrl { get; set; }

    public PlaylistVideo(string hash, string url, double duration, string title, string thumbnailUrl)
    {
        Hash = hash;
        Url = url;
        Duration = duration;
        Title = title;
        ThumbnailUrl = thumbnailUrl;
    }
}