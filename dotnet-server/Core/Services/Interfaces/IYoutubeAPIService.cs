namespace DotnetServer.Core.Services;

public interface IYouTubeAPIService
{
    int GetVideoDuration(string videoUrl);
    string GetVideoTitle(string videoUrl);
    string GetVideoThumbnailUrl(string videoUrl);
}