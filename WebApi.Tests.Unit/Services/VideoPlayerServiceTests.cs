using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebApi.Application.Services;
using WebApi.Application.Services.Interfaces;
using WebApi.Core.Entities;
using WebApi.Infrastructure.Repositories;
using WebApi.SignalR;

namespace WebApi.Tests.Unit.Services;

public class VideoPlayerServiceTests : IDisposable
{
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<ILogger<VideoPlayerService>> _mockLogger;
    private readonly Mock<IHubContext<AppHub>> _mockHubContext;
    private readonly Mock<IYouTubeAPIService> _mockYouTubeAPIService;
    private readonly Mock<IVideoPlayerStateService> _mockVideoStateService;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IRoomRepository> _mockRoomRepository;
    private readonly Mock<IPlaylistRepository> _mockPlaylistRepository;
    private readonly VideoPlayerService _videoPlayerService;

    public VideoPlayerServiceTests()
    {
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockLogger = new Mock<ILogger<VideoPlayerService>>();
        _mockHubContext = new Mock<IHubContext<AppHub>>();
        _mockYouTubeAPIService = new Mock<IYouTubeAPIService>();
        _mockVideoStateService = new Mock<IVideoPlayerStateService>();

        _mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        _mockRoomRepository = new Mock<IRoomRepository>();
        _mockPlaylistRepository = new Mock<IPlaylistRepository>();

        mockServiceProvider.Setup(x => x.GetService(typeof(IRoomRepository))).Returns(_mockRoomRepository.Object);
        mockServiceProvider.Setup(x => x.GetService(typeof(IPlaylistRepository))).Returns(_mockPlaylistRepository.Object);

        _mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);

        var mockClientProxy = new Mock<IClientProxy>();
        var mockHubClients = new Mock<IHubClients>();
        mockHubClients.Setup(x => x.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);
        _mockHubContext.Setup(x => x.Clients).Returns(mockHubClients.Object);

        _videoPlayerService = new VideoPlayerService(
            _mockScopeFactory.Object,
            _mockLogger.Object,
            _mockHubContext.Object,
            _mockYouTubeAPIService.Object,
            _mockVideoStateService.Object
        );
    }

    public void Dispose()
    {
        _videoPlayerService.IsServiceRunning = false;
    }

    [Fact]
    public void StartPlaylistService_ValidRoomWithVideos_ShouldStartSuccessfully()
    {
        var roomHash = "test-room";
        var room = new Room("TestRoom", "");
        room.PlaylistVideos.Add(new PlaylistVideo { Hash = "video1", Url = "https://youtube.com/watch?v=test1", Title = "Test Video" });
        room.Users.Add(new User { Username = "test-user" });

        _mockRoomRepository.Setup(x => x.GetRoomAsync(roomHash, It.IsAny<CancellationToken>())).ReturnsAsync(room);
        _mockYouTubeAPIService.Setup(x => x.GetVideoDurationAsync(It.IsAny<string>())).ReturnsAsync(120);

        _videoPlayerService.StartPlaylistService(roomHash);
        Thread.Sleep(200);

        _videoPlayerService.IsServiceRunning.Should().BeTrue();
        _videoPlayerService.StopPlaylistService(roomHash);
    }

    [Fact]
    public void StartPlaylistService_RoomDoesNotExist_ShouldStopService()
    {
        var roomHash = "nonexistent-room";
        _mockRoomRepository.Setup(x => x.GetRoomAsync(roomHash, It.IsAny<CancellationToken>())).ReturnsAsync((Room?)null);

        _videoPlayerService.StartPlaylistService(roomHash);
        Thread.Sleep(500);

        _mockRoomRepository.Verify(x => x.GetRoomAsync(roomHash, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void StartPlaylistService_EmptyPlaylist_ShouldStopService()
    {
        var roomHash = "test-room";
        var room = new Room("TestRoom", "");

        _mockRoomRepository.Setup(x => x.GetRoomAsync(roomHash, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        _videoPlayerService.StartPlaylistService(roomHash);
        Thread.Sleep(500);

        _mockRoomRepository.Verify(x => x.GetRoomAsync(roomHash, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void StopPlaylistService_ServiceRunning_ShouldStopSuccessfully()
    {
        var roomHash = "test-room";
        var room = new Room("TestRoom", "");
        room.PlaylistVideos.Add(new PlaylistVideo { Hash = "video1", Url = "https://youtube.com/watch?v=test1" });
        room.Users.Add(new User { Username = "test-user" });

        _mockRoomRepository.Setup(x => x.GetRoomAsync(roomHash, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        _videoPlayerService.StartPlaylistService(roomHash);
        Thread.Sleep(200);

        _videoPlayerService.StopPlaylistService(roomHash);
        Thread.Sleep(200);

        _videoPlayerService.IsServiceRunning.Should().BeFalse();
    }

    [Fact]
    public void StopPlaylistService_ServiceNotRunning_ShouldHandleGracefully()
    {
        var roomHash = "test-room";
        _videoPlayerService.IsServiceRunning = false;

        var action = () => _videoPlayerService.StopPlaylistService(roomHash);
        action.Should().NotThrow();
    }

    [Fact]
    public void StartPlaylistService_ConcurrentStarts_ShouldOnlyStartOnce()
    {
        var roomHash = "test-room";
        var room = new Room("TestRoom", "");
        room.PlaylistVideos.Add(new PlaylistVideo { Hash = "video1", Url = "https://youtube.com/watch?v=test1" });
        room.Users.Add(new User { Username = "test-user" });

        _mockRoomRepository.Setup(x => x.GetRoomAsync(roomHash, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        Parallel.Invoke(
            () => _videoPlayerService.StartPlaylistService(roomHash),
            () => _videoPlayerService.StartPlaylistService(roomHash),
            () => _videoPlayerService.StartPlaylistService(roomHash)
        );

        Thread.Sleep(500);
        _videoPlayerService.IsServiceRunning.Should().BeTrue();
        _videoPlayerService.StopPlaylistService(roomHash);
    }

    [Fact]
    public async Task RemovePlaylistVideoAsync_VideoExists_ShouldRemoveAndUpdateState()
    {
        var roomHash = "test-room";
        var videoHash = "video1";
        var video = new PlaylistVideo { Hash = videoHash, Url = "https://youtube.com/watch?v=test1", Title = "Test" };

        _mockPlaylistRepository.Setup(x => x.DeletePlaylistVideoAsync(roomHash, videoHash, It.IsAny<CancellationToken>())).ReturnsAsync(video);

        var result = await _videoPlayerService.RemovePlaylistVideoAsync(roomHash, videoHash, CancellationToken.None);

        result.Should().NotBeNull();
        result.Hash.Should().Be(videoHash);
        _mockVideoStateService.Verify(x => x.SetIsCurrentVideoRemoved(roomHash, true), Times.Once);
    }

    [Fact]
    public void IsServiceRunning_WhenServiceStops_ShouldBeFalse()
    {
        var roomHash = "test-room";
        var room = new Room("TestRoom", "");
        room.PlaylistVideos.Add(new PlaylistVideo { Hash = "video1", Url = "https://youtube.com/watch?v=test1" });
        room.Users.Add(new User { Username = "test-user" });

        _mockRoomRepository.Setup(x => x.GetRoomAsync(roomHash, It.IsAny<CancellationToken>())).ReturnsAsync(room);

        _videoPlayerService.StartPlaylistService(roomHash);
        Thread.Sleep(200);

        _videoPlayerService.IsServiceRunning.Should().BeTrue();

        _videoPlayerService.StopPlaylistService(roomHash);
        Thread.Sleep(200);

        _videoPlayerService.IsServiceRunning.Should().BeFalse();
    }
}
