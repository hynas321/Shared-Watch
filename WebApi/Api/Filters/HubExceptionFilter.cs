using Microsoft.AspNetCore.SignalR;

namespace WebApi.Api.Filters;

public class HubExceptionFilter : IHubFilter
{
    private readonly ILogger<HubExceptionFilter> _logger;

    public HubExceptionFilter(ILogger<HubExceptionFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object>> next)
    {
        try
        {
            return await next(invocationContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Hub: '{Hub}', method: '{Method}'",
                invocationContext.Hub.GetType().Name,
                invocationContext.HubMethodName);

            throw new HubException("An internal server error occurred");
        }
    }
}
