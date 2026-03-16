using Google.Apis.Services;
using System.Reflection;
using WebApi.Application.Services;

namespace WebApi.Tests.Unit.Services;

public class YouTubeAPIServiceTests
{
    private readonly Mock<BaseClientService.Initializer> _mockInitializer;

    public YouTubeAPIServiceTests()
    {
        _mockInitializer = new Mock<BaseClientService.Initializer>();
    }

    private YouTubeAPIService CreateSut()
    {
        return new YouTubeAPIService(_mockInitializer.Object);
    }

    private string? CallGetVideoId(YouTubeAPIService service, string url)
    {
        var method = service.GetType().GetMethod("GetVideoId", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = method?.Invoke(service, [url]);
        return result as string;
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/v/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    public void GetVideoId_ValidYouTubeUrls_ShouldExtractCorrectId(string url, string expectedId)
    {
        var service = CreateSut();
        var result = CallGetVideoId(service, url);
        result.Should().Be(expectedId);
    }

    [Theory]
    [InlineData("https://www.example.com/video")]
    [InlineData("https://google.com")]
    [InlineData("not a url")]
    [InlineData("")]
    public void GetVideoId_InvalidUrls_ShouldReturnNull(string url)
    {
        var service = CreateSut();
        var result = CallGetVideoId(service, url);
        result.Should().BeNull();
    }

    [Fact]
    public void GetVideoId_NullUrl_ShouldReturnNull()
    {
        var service = CreateSut();
        var result = CallGetVideoId(service, null!);
        result.Should().BeNull();
    }

    [Fact]
    public void GetVideoId_VideoIdLessThan11Chars_ShouldReturnNull()
    {
        var service = CreateSut();
        var result = CallGetVideoId(service, "https://www.youtube.com/watch?v=abc");
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("https://youtu.be/dQw4w9WgXcQ?t=10", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLtest&index=1", "dQw4w9WgXcQ")]
    public void GetVideoId_UrlWithParameters_ShouldExtractCorrectId(string url, string expectedId)
    {
        var service = CreateSut();
        var result = CallGetVideoId(service, url);
        result.Should().Be(expectedId);
    }

    [Fact]
    public void GetVideoId_CaseInsensitiveUrl_ShouldExtractCorrectId()
    {
        var service = CreateSut();
        var result = CallGetVideoId(service, "HTTPS://YOUTU.BE/DQW4W9WGXCQ");
        result.Should().Be("DQW4W9WGXCQ");
    }
}
