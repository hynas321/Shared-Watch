using System.Collections.Concurrent;

namespace WebApi.Api.SignalR;

public class HubConnectionMapper
{
    public static readonly ConcurrentDictionary<string, string> UserConnections = new ConcurrentDictionary<string, string>();
}
