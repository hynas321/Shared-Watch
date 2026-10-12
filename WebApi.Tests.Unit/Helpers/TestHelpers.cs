using Microsoft.Extensions.Options;
using WebApi.Application.Options;

namespace WebApi.Tests.Unit.Helpers;

public static class TestHelpers
{
    public static IOptions<JwtOptions> CreateJwtOptions(string key = "test_key_1234567890123456789012345abc", string issuer = "test_issuer")
    {
        var options = new JwtOptions { Key = key, Issuer = issuer };
        return Microsoft.Extensions.Options.Options.Create(options);
    }

    public static Mock<ILogger<T>> CreateLoggerMock<T>() where T : class
    {
        return new Mock<ILogger<T>>();
    }

    public static CancellationToken GetCancellationToken()
    {
        return CancellationToken.None;
    }
}
