using Google.Apis.Services;
using Google.Apis.YouTube.v3;

public class YouTubeAPIService
{
    private readonly YouTubeService _youTubeService;

    public YouTubeAPIService(BaseClientService.Initializer initializer)
    {
        _youTubeService = new YouTubeService(initializer);
    }

    public string GetVideoDuration(string videoId)
    {
        var videoRequest = _youTubeService.Videos.List("contentDetails");
        videoRequest.Id = videoId;

        var videoResponse = videoRequest.Execute();

        if (videoResponse.Items.Count == 1)
        {
            var duration = videoResponse.Items[0].ContentDetails.Duration;
            return duration;
        }

        return null;
    }
}