using System.Security.Claims;
using WebApi.Api.Services.Interfaces;

namespace WebApi.Api.Services;

public class HttpContextService : IHttpContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetUsername()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
    }

    public string GetRole()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
    }

    public string GetRoomHash()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Hash);
    }
}


