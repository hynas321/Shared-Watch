using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.SignalR;
using WebApi.Application.Services;
using WebApi.Application.Options;
using WebApi.SignalR;
using WebApi.Core.Entities;

namespace WebApi.Controllers;

[ApiController]
[Route("api/VideoFiles")]
public class VideoFilesController : ControllerBase
{
    private const long MaxVideoSizeBytes = VideoStorageOptions.MaximumSupportedUploadBytes;

    private readonly IUploadedVideoStorage _storage;
    private readonly VideoFileApplicationService _videoFileService;
    private readonly IVideoPlayerService _videoPlayerService;
    private readonly IHubContext<AppHub> _hubContext;
    private readonly ILogger<VideoFilesController> _logger;

    public VideoFilesController(
        IUploadedVideoStorage storage,
        VideoFileApplicationService videoFileService,
        IVideoPlayerService videoPlayerService,
        IHubContext<AppHub> hubContext,
        ILogger<VideoFilesController> logger)
    {
        _storage = storage;
        _videoFileService = videoFileService;
        _videoPlayerService = videoPlayerService;
        _hubContext = hubContext;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("{roomHash}")]
    [RequestSizeLimit(MaxVideoSizeBytes + 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxVideoSizeBytes + 1024 * 1024)]
    public async Task<IActionResult> Upload(
        string roomHash,
        [FromForm] IFormFile? file,
        [FromForm] double duration,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(User.FindFirstValue(ClaimTypes.Hash), roomHash, StringComparison.Ordinal))
        {
            return Forbid();
        }

        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("Video upload requested for room {RoomHash} by user {Username}. FileName: {FileName}, SizeBytes: {SizeBytes}, DurationSeconds: {DurationSeconds}, RequestId: {RequestId}", roomHash, username, file?.FileName, file?.Length, duration, HttpContext.TraceIdentifier);
        if (file is null || string.IsNullOrWhiteSpace(username))
        {
            return BadRequest();
        }

        await using var content = file.OpenReadStream();
        var result = await _videoFileService.UploadAsync(
            roomHash,
            username,
            content,
            file.FileName,
            file.Length,
            duration,
            cancellationToken);

        _logger.LogInformation("Video upload completed for room {RoomHash} by user {Username}. Status: {Status}, Message: {Message}, RequestId: {RequestId}", roomHash, username, result.Status, result.Message, HttpContext.TraceIdentifier);

        if (result.Status == VideoFileUploadStatus.Added)
        {
            _videoPlayerService.StartPlaylistService(roomHash);
        }

        return result.Status switch
        {
            VideoFileUploadStatus.Added => await BroadcastUploadedVideoAsync(roomHash, result.Video!, cancellationToken),
            VideoFileUploadStatus.RoomNotFound => NotFound(),
            VideoFileUploadStatus.Forbidden => Forbid(),
            VideoFileUploadStatus.PlaylistFull => Conflict(new { message = result.Message }),
            VideoFileUploadStatus.InvalidFile or VideoFileUploadStatus.InvalidDuration => BadRequest(new { message = result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [Authorize]
    [HttpGet("{roomHash}/{fileName}/access")]
    public async Task<IActionResult> GetStreamAccessUrl(
        string roomHash,
        string fileName,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(User.FindFirstValue(ClaimTypes.Hash), roomHash, StringComparison.Ordinal))
        {
            return Forbid();
        }

        var username = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(username))
        {
            return Forbid();
        }

        var result = await _videoFileService.CreateStreamAccessAsync(
            roomHash,
            fileName,
            username,
            cancellationToken);

        return result.Status switch
        {
            VideoStreamAccessStatus.Granted => Ok(new { url = result.Url }),
            VideoStreamAccessStatus.NotFound => NotFound(),
            VideoStreamAccessStatus.Forbidden => Forbid(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpGet("{roomHash}/{fileName}")]
    public IActionResult Stream(string roomHash, string fileName, long expires, string signature)
    {
        if (!_videoFileService.IsStreamSignatureValid(roomHash, fileName, expires, signature))
        {
            return Unauthorized();
        }

        var stream = _storage.OpenRead(fileName);
        if (stream is null)
        {
            return NotFound();
        }

        var contentTypeProvider = new FileExtensionContentTypeProvider();
        var contentType = contentTypeProvider.TryGetContentType(fileName, out var detectedType)
            ? detectedType
            : "application/octet-stream";
        return File(stream, contentType, enableRangeProcessing: true);
    }

    private async Task<IActionResult> BroadcastUploadedVideoAsync(
        string roomHash,
        PlaylistVideo video,
        CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(roomHash)
            .SendAsync(HubMessages.OnAddPlaylistVideo, video, cancellationToken);
        return Ok(video);
    }
}

