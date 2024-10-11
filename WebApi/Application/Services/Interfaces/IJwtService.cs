using WebApi.Application.Models;

namespace WebApi.Application.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(string username, string role, string roomHash);
}
