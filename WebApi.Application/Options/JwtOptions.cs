namespace WebApi.Application.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string? Key { get; set; }
    public string? Issuer { get; set; }
    public string? Authority { get; set; }
}

