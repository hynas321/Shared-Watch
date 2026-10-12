using Microsoft.AspNetCore.Mvc.Controllers;
using System.Diagnostics;

namespace WebApi.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        Endpoint? endpoint = context.GetEndpoint();
        ControllerActionDescriptor? descriptor =
            endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

        if (descriptor is null)
        {
            await _next(context);
            return;
        }

        string controller = descriptor.ControllerName;
        string action = descriptor.ActionName;
        string path = context.Request.Path + context.Request.QueryString;
        if (context.Request.Query.ContainsKey("signature"))
        {
            path = context.Request.Path + "?[signed media query redacted]";
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var level = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

            _logger.Log(
                level,
                "HTTP {StatusCode} {Method} {Path} ({Controller}/{Action}) | Elapsed: {Elapsed} ms | TraceId: {TraceId} | User: {User}",
                statusCode,
                context.Request.Method,
                path,
                controller,
                action,
                stopwatch.ElapsedMilliseconds,
                context.TraceIdentifier,
                context.User.Identity?.Name ?? "anonymous");
        }
    }
}
