using Microsoft.AspNetCore.Http;
using WebApi.Api.Middleware;
using WebApi.Tests.Unit.Helpers;

namespace WebApi.Tests.Unit.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;
    private readonly Mock<RequestDelegate> _mockRequestDelegate;
    private readonly ExceptionHandlingMiddleware _middleware;

    public ExceptionHandlingMiddlewareTests()
    {
        _mockLogger = TestHelpers.CreateLoggerMock<ExceptionHandlingMiddleware>();
        _mockRequestDelegate = new Mock<RequestDelegate>();
        _middleware = new ExceptionHandlingMiddleware(_mockRequestDelegate.Object, _mockLogger.Object);
    }

    private DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    [Fact]
    public async Task Invoke_NoException_ShouldCallNextDelegate()
    {
        var httpContext = CreateHttpContext();
        _mockRequestDelegate.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        await _middleware.Invoke(httpContext);

        _mockRequestDelegate.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        httpContext.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Invoke_ExceptionThrown_ShouldReturn500StatusCode()
    {
        var httpContext = CreateHttpContext();
        _mockRequestDelegate.Setup(x => x(It.IsAny<HttpContext>())).Throws(new Exception("Test exception"));

        await _middleware.Invoke(httpContext);

        httpContext.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Invoke_ExceptionThrown_ShouldLogError()
    {
        var httpContext = CreateHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/test/path";
        _mockRequestDelegate.Setup(x => x(It.IsAny<HttpContext>())).Throws(new Exception("Test exception"));

        await _middleware.Invoke(httpContext);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_GenericException_ShouldHandleGracefully()
    {
        var httpContext = CreateHttpContext();
        _mockRequestDelegate.Setup(x => x(It.IsAny<HttpContext>())).Throws(new InvalidOperationException("Invalid operation"));

        await _middleware.Invoke(httpContext);

        httpContext.Response.StatusCode.Should().Be(500);
    }
}
