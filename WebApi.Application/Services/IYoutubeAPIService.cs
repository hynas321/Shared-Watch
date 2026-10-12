namespace WebApi.Application.Services;

public interface IYouTubeAPIService
{
    Task<int> GetVideoDurationAsync(string videoUrl);
    Task<int> GetVideoDurationAsync(string videoUrl, CancellationToken cancellationToken);
    Task<string?> GetVideoTitleAsync(string videoUrl);
    Task<string?> GetVideoThumbnailUrlAsync(string videoUrl);

}
