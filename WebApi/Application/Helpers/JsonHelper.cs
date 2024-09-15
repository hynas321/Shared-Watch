using System.Text.Json;

namespace WebApi.Shared.Helpers;

class JsonHelper
{
    public static string Serialize(object o)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(o, jsonSerializerOptions);
    }
}