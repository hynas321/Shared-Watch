namespace WebApi.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(string username, string role, string roomHash);
}
