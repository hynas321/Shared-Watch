namespace WebApi.Api.Services;

public interface IHttpContextService
{
    string? GetUsername();
    string? GetRole();
    string? GetRoomHash();
}
