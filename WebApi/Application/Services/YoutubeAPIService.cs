using System.Text.RegularExpressions;
using System.Xml;
using WebApi.Application.Services.Interfaces;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace WebApi.Application.Services;

public class YouTubeAPIService : IYouTubeAPIService
{
    private readonly YouTubeService _youTubeService;

    public YouTubeAPIService(BaseClientService.Initializer initializer)
    {
        _youTubeService = new YouTubeService(initializer);
    }

    public async Task<int> GetVideoDurationAsync(string videoUrl)
    {
        return await Task.Run(() =>
        {
            var videoRequest = _youTubeService.Videos.List("contentDetails");
            videoRequest.Id = GetVideoId(videoUrl);

            var videoResponse = videoRequest.Execute();

            if (videoResponse.Items.Count == 1)
            {
                var duration = videoResponse.Items[0].ContentDetails.Duration;
                var timeSpan = XmlConvert.ToTimeSpan(duration);

                return (int)timeSpan.TotalSeconds;
            }

            return -1;
        });
    }

    public async Task<string> GetVideoTitleAsync(string videoUrl)
    {
        return await Task.Run(() =>
        {
            var videoRequest = _youTubeService.Videos.List("snippet");
            videoRequest.Id = GetVideoId(videoUrl);

            var videoResponse = videoRequest.Execute();

            if (videoResponse.Items.Count == 1)
            {
                string title = videoResponse.Items[0].Snippet.Title;
                return title;
            }

            return null;
        });
    }

    public async Task<string> GetVideoThumbnailUrlAsync(string videoUrl)
    {
        return await Task.Run(() =>
        {
            var videoRequest = _youTubeService.Videos.List("snippet");
            videoRequest.Id = GetVideoId(videoUrl);

            var videoResponse = videoRequest.Execute();

            if (videoResponse.Items.Count == 1)
            {
                var thumbnailUrl = videoResponse.Items[0].Snippet.Thumbnails.Default__.Url;
                return thumbnailUrl;
            }

            return null;
        });
    }

    private string GetVideoId(string videoUrl)
    {
        var regex = new Regex(@"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})", RegexOptions.IgnoreCase);

        var match = regex.Match(videoUrl);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }
}