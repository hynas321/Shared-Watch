public interface IPlaylistVideo
{
    string Hash { get; set; }
    string Url { get; set; }
    double Duration { get; set; }
    string Title { get; set; }
    string ThumbnailUrl { get; set; }
}