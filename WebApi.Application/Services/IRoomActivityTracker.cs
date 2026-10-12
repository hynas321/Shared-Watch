namespace WebApi.Application.Services;
public interface IRoomActivityTracker
{
    Task TouchAsync(string roomHash);
    Task<bool> IsInactiveAsync(string roomHash, TimeSpan inactivityPeriod);
    Task RemoveAsync(string roomHash);
}
