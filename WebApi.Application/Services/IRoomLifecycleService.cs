namespace WebApi.Application.Services;
public interface IRoomLifecycleService
{
    Task<bool> DeleteIfEmptyAsync(string roomHash, CancellationToken cancellationToken);
}
