namespace WebApi.Application.Services;
public sealed record StoredVideoFile(string FileName, DateTime LastModifiedUtc);
public interface IUploadedVideoStorage
{
    Task SaveAsync(string fileName, Stream content, CancellationToken cancellationToken);
    Task DeleteAsync(string fileName, CancellationToken cancellationToken);
    Stream? OpenRead(string fileName);
    IReadOnlyCollection<StoredVideoFile> ListFiles();
}
