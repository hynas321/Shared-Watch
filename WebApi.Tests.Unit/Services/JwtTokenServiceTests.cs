using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApi.Application.Services;
using WebApi.Core.Entities.Static;
using WebApi.Tests.Unit.Helpers;

namespace WebApi.Tests.Unit.Services;

public class JwtTokenServiceTests
{
    private readonly IOptions<JwtOptions> _jwtOptions;
    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        _jwtOptions = TestHelpers.CreateJwtOptions();
        _jwtTokenService = new JwtTokenService(_jwtOptions);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidToken()
    {
        string token = _jwtTokenService.GenerateToken("testuser", "User", "room123");
        token.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "testuser");
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "User");
        jsonToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Hash && c.Value == "room123");
    }

    [Fact]
    public void GenerateToken_ShouldHaveCorrectIssuer()
    {
        string token = _jwtTokenService.GenerateToken("testuser", "User", "room123");

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.Issuer.Should().Be("test_issuer");
    }

    [Fact]
    public void GenerateToken_ShouldHaveExpirationInFuture()
    {
        DateTime now = DateTime.Now;
        string token = _jwtTokenService.GenerateToken("testuser", "User", "room123");

        var tokenHandler = new JwtSecurityTokenHandler();
        var jsonToken = tokenHandler.ReadJwtToken(token);

        jsonToken.ValidTo.Should().BeAfter(now.AddHours(11).ToUniversalTime());
    }
}
