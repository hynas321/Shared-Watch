namespace DotnetServer.Application.Services.Interfaces;

public interface IYouTubeAPIService
{
    Task<int> GetVideoDurationAsync(string videoUrl);
    Task<string> GetVideoTitleAsync(string videoUrl);
    Task<string> GetVideoThumbnailUrlAsync(string videoUrl);

}