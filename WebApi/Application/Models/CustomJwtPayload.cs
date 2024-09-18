namespace WebApi.Application.Models;

public class CustomJwtPayload
{
    public string NameIdentifier { get; set; }
    public string Role { get; set; }
    public string Hash { get; set; }
    public long? Exp { get; set; }
    public string Iss { get; set; }
    public string Aud { get; set; }
}