using System.ComponentModel.DataAnnotations;

namespace WebApi.Core.Entities;

public class PlaylistVideo
{
    [Key]
    public string Hash { get; set; } = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);

    public string Url { get; set; }
    public double Duration { get; set; }
    public string Title { get; set; }
    public string ThumbnailUrl { get; set; }

    // Foreign Key
    public string RoomHash { get; set; } 

    public PlaylistVideo() { }

    public PlaylistVideo(string hash, string url, double duration, string title, string thumbnailUrl)
    {
        Hash = hash ?? Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        Url = url;
        Duration = duration;
        Title = title;
        ThumbnailUrl = thumbnailUrl;
    }
}