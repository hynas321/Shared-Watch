namespace WebApi.Api.Services.Interfaces;

public interface IHttpContextService
{
    string GetUsername();
    string GetRole();
    string GetRoomHash();
}
