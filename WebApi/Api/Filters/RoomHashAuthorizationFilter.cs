using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Api.Filters;

public class RoomHashAuthorizationFilter : IHubFilter
{
    public async ValueTask<object> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object>> next)
    {
        var userHashClaim = invocationContext.Context.User?.FindFirst(ClaimTypes.Hash)?.Value;
        var roomHashArgument = invocationContext.HubMethodArguments.FirstOrDefault();
        var roomHash = roomHashArgument?.ToString();

        if (userHashClaim == null)
        {
            throw new HubException("Unauthorized: Missing Hash claim.");
        }

        if (roomHash == null)
        {
            throw new HubException("Invalid request: Missing roomHash parameter.");
        }

        if (userHashClaim != roomHash)
        {
            throw new HubException("Unauthorized: Hash claim does not match roomHash.");
        }

        return await next(invocationContext);
    }
}
