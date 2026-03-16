using WebApi.Application.Services.Interfaces;
using WebApi.Core.Entities;

namespace WebApi.Tests.Unit.Services;

public class VideoPlayerStateServiceTests : IDisposable
{
    private readonly VideoPlayerStateService _stateService;

    public VideoPlayerStateServiceTests()
    {
        _stateService = new VideoPlayerStateService();
    }

    public void Dispose() { }

    [Fact]
    public void SetIsPlaying_ValidRoomHash_ShouldSetState()
    {
        _stateService.SetIsPlaying("test-room", true);
        _stateService.GetIsPlaying("test-room").Should().BeTrue();
    }

    [Fact]
    public void SetCurrentTime_ValidRoomHash_ShouldSetTime()
    {
        _stateService.SetCurrentTime("test-room", 123.45);
        _stateService.GetCurrentTime("test-room").Should().Be(123.45);
    }

    [Fact]
    public void SetCurrentVideo_ValidVideo_ShouldSetVideo()
    {
        var video = new PlaylistVideo { Hash = "video-hash", Url = "https://youtube.com/watch?v=test", Title = "Test Video" };
        _stateService.SetCurrentVideo("test-room", video);
        _stateService.GetCurrentVideo("test-room")!.Hash.Should().Be("video-hash");
    }

    [Fact]
    public void SetIsCurrentVideoRemoved_ValidRoomHash_ShouldSetFlag()
    {
        _stateService.SetIsCurrentVideoRemoved("test-room", true);
        _stateService.GetIsCurrentVideoRemoved("test-room").Should().BeTrue();
    }

    [Fact]
    public void GetVideoPlayer_CompleteState_ShouldReturnFullPlayer()
    {
        var video = new PlaylistVideo { Hash = "video-hash", Url = "url", Title = "Title" };
        _stateService.SetCurrentVideo("test-room", video);
        _stateService.SetIsPlaying("test-room", true);
        _stateService.SetCurrentTime("test-room", 45.5);

        var player = _stateService.GetVideoPlayer("test-room");
        player.Should().NotBeNull();
        player!.IsPlaying.Should().BeTrue();
        player.CurrentTime.Should().Be(45.5);
    }
}
