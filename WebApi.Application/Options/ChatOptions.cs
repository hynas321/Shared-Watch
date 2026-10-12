namespace WebApi.Application.Options;

public sealed class ChatOptions
{
    public const string SectionName = "Chat";

    public int MaxMessages { get; set; } = 30;

    public int MaxMessageTextLength { get; set; } = 200;
}

