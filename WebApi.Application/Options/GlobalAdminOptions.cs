namespace WebApi.Application.Options;

public sealed class GlobalAdminOptions
{
    public const string ConfigurationKey = "GlobalAdminToken";

    public string Token { get; set; } = string.Empty;
}
