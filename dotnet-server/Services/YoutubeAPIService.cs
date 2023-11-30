using System.Text.RegularExpressions;
using System.Xml;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

public class YouTubeAPIService
{
    private readonly YouTubeService _youTubeService;

    public YouTubeAPIService(BaseClientService.Initializer initializer)
    {
        _youTubeService = new YouTubeService(initializer);
    }

    [Obsolete]
    public int GetVideoDuration(string videoUrl)
    {
        var videoRequest = _youTubeService.Videos.List("contentDetails");
        videoRequest.Id = GetVideoId(videoUrl);

        var videoResponse = videoRequest.Execute();

        if (videoResponse.Items.Count == 1)
        {
            string duration = videoResponse.Items[0].ContentDetails.Duration;
            
            TimeSpan timeSpan = XmlConvert.ToTimeSpan(duration);

            if ((int)timeSpan.TotalSeconds == 0)
            {
                (bool isLive, double currentTime) videoIsLiveOutput = CheckIfVideoIsLive(videoUrl);

                if (!videoIsLiveOutput.isLive)
                {
                    return -1;
                }

                return (int)videoIsLiveOutput.currentTime;
            }

            return (int)timeSpan.TotalSeconds;
        }

        return -1;
    }

    public string GetVideoTitle(string videoUrl)
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
    }

    public string GetVideoThumbnailUrl(string videoUrl)
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

    [Obsolete]
    private (bool, double) CheckIfVideoIsLive(string videoUrl)
    {
        var videoRequest = _youTubeService.Videos.List("liveStreamingDetails");
        videoRequest.Id = GetVideoId(videoUrl);

        var videoResponse = videoRequest.Execute();

        if (videoResponse.Items.Count == 1)
        {
            var liveStreamingDetails = videoResponse.Items[0].LiveStreamingDetails;

            if (liveStreamingDetails != null && liveStreamingDetails.ActualStartTime != null)
            {
                DateTime currentTime = DateTime.Now.ToUniversalTime();
                DateTime startTime = liveStreamingDetails.ActualStartTime.Value;

                TimeSpan elapsedTime = currentTime - startTime;
                return (true, elapsedTime.TotalSeconds);
            }

            return (false, -1);
        }

        return (false, -1);
    }
}