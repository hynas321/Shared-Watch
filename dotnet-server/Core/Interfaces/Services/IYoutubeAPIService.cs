public interface IYouTubeAPIService
{
    public int GetVideoDuration(string videoUrl);
    public string GetVideoTitle(string videoUrl);
    public string GetVideoThumbnailUrl(string videoUrl);
}