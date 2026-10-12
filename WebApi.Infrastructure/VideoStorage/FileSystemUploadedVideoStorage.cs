using Microsoft.Extensions.Options;
using WebApi.Application.Options;
using WebApi.Application.Services;
namespace WebApi.Infrastructure.VideoStorage;
public sealed class FileSystemUploadedVideoStorage : IUploadedVideoStorage
{
    private readonly string _root;
    public FileSystemUploadedVideoStorage(IOptions<VideoStorageOptions> options)
    {
        var configured = options.Value.Path;
        _root = Path.GetFullPath(Path.IsPathRooted(configured) ? configured : Path.Combine(AppContext.BaseDirectory, configured));
        Directory.CreateDirectory(_root);
    }
    public async Task SaveAsync(string fileName, Stream content, CancellationToken cancellationToken)
    {
        await using var target = new FileStream(GetPath(fileName), FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous);
        await content.CopyToAsync(target, cancellationToken);
    }
    public Task DeleteAsync(string fileName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = GetPath(fileName);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
    public Stream? OpenRead(string fileName)
    {
        var path = GetPath(fileName);
        return File.Exists(path) ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan) : null;
    }
    public IReadOnlyCollection<StoredVideoFile> ListFiles() => Directory.EnumerateFiles(_root)
        .Select(path => new FileInfo(path)).Select(file => new StoredVideoFile(file.Name, file.LastWriteTimeUtc)).ToArray();
    private string GetPath(string fileName)
    {
        var safe = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safe) || safe != fileName) throw new ArgumentException("Invalid stored video file name.", nameof(fileName));
        return Path.Combine(_root, safe);
    }
}

