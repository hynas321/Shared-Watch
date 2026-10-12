namespace WebApi.Application.Options;
public sealed class VideoStorageOptions
{
    public const string SectionName = "VideoStorage";
    public const long MaximumSupportedUploadBytes = 500L * 1024 * 1024;
    public string Path { get; set; } = "video-storage";
    public long MaxFileSizeBytes { get; set; } = MaximumSupportedUploadBytes;
    public double MaxDurationSeconds { get; set; } = 86400;
}
