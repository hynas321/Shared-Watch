using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using WebApi.Application.Repositories;
using WebApi.Application.Services;
using WebApi.Application.Options;
using WebApi.Core.Entities;

namespace WebApi.Application.Services;

public enum VideoFileUploadStatus
{
    Added,
    RoomNotFound,
    Forbidden,
    PlaylistFull,
    InvalidFile,
    InvalidDuration
}

public sealed record VideoFileUploadResult(
    VideoFileUploadStatus Status,
    PlaylistVideo? Video = null,
    string? Message = null);

public enum VideoStreamAccessStatus
{
    Granted,
    NotFound,
    Forbidden
}

public sealed record VideoStreamAccessResult(VideoStreamAccessStatus Status, string? Url = null);

public sealed class VideoFileApplicationService
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".webm", ".ogg"
    };

    private readonly IRoomRepository _roomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IUploadedVideoStorage _storage;
    private readonly VideoStorageOptions _options;
    private readonly byte[] _signatureKey;

    public VideoFileApplicationService(
        IRoomRepository roomRepository,
        IUserRepository userRepository,
        IPlaylistRepository playlistRepository,
        IUploadedVideoStorage storage,
        IOptions<VideoStorageOptions> options,
        IOptions<JwtOptions> jwtOptions)
    {
        _roomRepository = roomRepository;
        _userRepository = userRepository;
        _playlistRepository = playlistRepository;
        _storage = storage;
        _options = options.Value;
        var configuredKey = jwtOptions.Value.Key;
        _signatureKey = Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(configuredKey)
            ? "shared-watch-video-signing-key"
            : configuredKey);
    }

    public async Task<VideoFileUploadResult> UploadAsync(
        string roomHash,
        string username,
        Stream content,
        string fileName,
        long length,
        double duration,
        CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);
        if (room is null)
            return new(VideoFileUploadStatus.RoomNotFound);

        if (await _userRepository.GetUserAsync(roomHash, username, cancellationToken) is null)
            return new(VideoFileUploadStatus.Forbidden);

        var extension = Path.GetExtension(fileName);
        if (!SupportedExtensions.Contains(extension) || length <= 0 || length > _options.MaxFileSizeBytes)
            return new(VideoFileUploadStatus.InvalidFile, Message: "The file type or size is invalid.");

        if (duration <= 0 || duration > _options.MaxDurationSeconds)
            return new(VideoFileUploadStatus.InvalidDuration, Message: "The video duration is invalid.");

        var storedFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        await _storage.SaveAsync(storedFileName, content, cancellationToken);

        var video = new PlaylistVideo(
            Guid.NewGuid().ToString("N"),
            $"/api/VideoFiles/{Uri.EscapeDataString(roomHash)}/{Uri.EscapeDataString(storedFileName)}",
            duration,
            Path.GetFileNameWithoutExtension(fileName),
            string.Empty)
        {
            RoomHash = roomHash
        };

        var addResult = await _playlistRepository.AddPlaylistVideoAsync(roomHash, video, cancellationToken);
        if (addResult == PlaylistVideoAddResult.RoomNotFound)
        {
            await _storage.DeleteAsync(storedFileName, cancellationToken);
            return new(VideoFileUploadStatus.RoomNotFound);
        }

        if (addResult == PlaylistVideoAddResult.PlaylistFull)
        {
            await _storage.DeleteAsync(storedFileName, cancellationToken);
            return new(VideoFileUploadStatus.PlaylistFull, Message: "The playlist is full.");
        }

        return new(VideoFileUploadStatus.Added, video);
    }

    public async Task<VideoStreamAccessResult> CreateStreamAccessAsync(
        string roomHash,
        string fileName,
        string username,
        CancellationToken cancellationToken)
    {
        if (await _userRepository.GetUserAsync(roomHash, username, cancellationToken) is null)
            return new(VideoStreamAccessStatus.Forbidden);

        var playlistVideoUrl =
            $"/api/VideoFiles/{Uri.EscapeDataString(roomHash)}/{Uri.EscapeDataString(fileName)}";
        if (!await _playlistRepository.RoomHasPlaylistVideoUrlAsync(roomHash, playlistVideoUrl, cancellationToken))
            return new(VideoStreamAccessStatus.NotFound);

        var expires = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
        var signature = CreateSignature(roomHash, fileName, expires);
        var url = $"/api/VideoFiles/{Uri.EscapeDataString(roomHash)}/{Uri.EscapeDataString(fileName)}?expires={expires}&signature={Uri.EscapeDataString(signature)}";
        return new(VideoStreamAccessStatus.Granted, url);
    }

    public bool IsStreamSignatureValid(string roomHash, string fileName, long expires, string signature)
    {
        if (expires <= DateTimeOffset.UtcNow.ToUnixTimeSeconds() || string.IsNullOrWhiteSpace(signature))
            return false;

        var expected = CreateSignature(roomHash, fileName, expires);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature));
    }

    private string CreateSignature(string roomHash, string fileName, long expires)
    {
        var payload = $"{roomHash}\n{fileName}\n{expires}";
        return Convert.ToHexString(HMACSHA256.HashData(_signatureKey, Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }
}


