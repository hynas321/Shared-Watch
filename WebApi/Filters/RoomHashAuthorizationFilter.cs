using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using WebApi.Application.Repositories;
using WebApi.Application.Services;
using WebApi.SignalR;

namespace WebApi.Filters;

public class RoomHashAuthorizationFilter : IHubFilter
{
    private readonly IUserRepository _userRepository;
    private readonly IRoomActivityTracker _activityTracker;

    public RoomHashAuthorizationFilter(
        IUserRepository userRepository,
        IRoomActivityTracker activityTracker)
    {
        _userRepository = userRepository;
        _activityTracker = activityTracker;
    }

    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var userHashClaim = invocationContext.Context.User?.FindFirst(ClaimTypes.Hash)?.Value;
        var roomHashArgument = invocationContext.HubMethodArguments.FirstOrDefault();
        var roomHash = roomHashArgument?.ToString();

        if (roomHash is null)
        {
            throw new HubException("A room identifier is required.");
        }

        if (userHashClaim is null || !string.Equals(userHashClaim, roomHash, StringComparison.Ordinal))
        {
            throw new HubException("You are not authorized to access this room.");
        }

        var username = invocationContext.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(username) ||
            await _userRepository.GetUserAsync(roomHash, username, invocationContext.Context.ConnectionAborted) is null)
        {
            throw new HubException("Your room access is no longer active.");
        }

        if (!string.Equals(
                invocationContext.HubMethodName,
                HubMessages.RefreshConnectionPresence,
                StringComparison.Ordinal))
        {
            await _activityTracker.TouchAsync(roomHash);
        }
        return await next(invocationContext);
    }
}

