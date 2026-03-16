using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.Api.Filters;

public class RoomHashAuthorizationFilter : IHubFilter
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var userHashClaim = invocationContext.Context.User?.FindFirst(ClaimTypes.Hash)?.Value;
        var roomHashArgument = invocationContext.HubMethodArguments.FirstOrDefault();
        var roomHash = roomHashArgument?.ToString();

        if (roomHash is null)
        {
            return await next(invocationContext);
        }

        if (userHashClaim is null)
        {
            return await next(invocationContext);
        }

        if (userHashClaim != roomHash)
        {
            return await next(invocationContext);
        }

        return await next(invocationContext);
    }
}
