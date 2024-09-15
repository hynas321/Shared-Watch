namespace WebApi.Application.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(string username, string role, string roomHash);
}
