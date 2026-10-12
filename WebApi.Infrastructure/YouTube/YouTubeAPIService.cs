using System.Text.RegularExpressions;
using System.Xml;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using WebApi.Application.Services;

namespace WebApi.Infrastructure.YouTube;

public sealed class YouTubeAPIService : IYouTubeAPIService
{
    private readonly YouTubeService _youTubeService;
    public YouTubeAPIService(BaseClientService.Initializer initializer) => _youTubeService = new YouTubeService(initializer);

    public Task<int> GetVideoDurationAsync(string videoUrl) => GetVideoDurationAsync(videoUrl, CancellationToken.None);

    public async Task<int> GetVideoDurationAsync(string videoUrl, CancellationToken cancellationToken)
    {
        var request = _youTubeService.Videos.List("contentDetails");
        request.Id = GetVideoId(videoUrl);
        var response = await request.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        return response.Items.Count == 1 ? (int)XmlConvert.ToTimeSpan(response.Items[0].ContentDetails.Duration).TotalSeconds : -1;
    }

    public async Task<string?> GetVideoTitleAsync(string videoUrl)
    {
        var request = _youTubeService.Videos.List("snippet");
        request.Id = GetVideoId(videoUrl);
        var response = await request.ExecuteAsync().ConfigureAwait(false);
        return response.Items.Count == 1 ? response.Items[0].Snippet.Title : null;
    }

    public async Task<string?> GetVideoThumbnailUrlAsync(string videoUrl)
    {
        var request = _youTubeService.Videos.List("snippet");
        request.Id = GetVideoId(videoUrl);
        var response = await request.ExecuteAsync().ConfigureAwait(false);
        return response.Items.Count == 1 ? response.Items[0].Snippet.Thumbnails.Default__?.Url : null;
    }

    private static string? GetVideoId(string videoUrl)
    {
        if (string.IsNullOrWhiteSpace(videoUrl)) return null;
        var match = Regex.Match(videoUrl, @"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }
}
