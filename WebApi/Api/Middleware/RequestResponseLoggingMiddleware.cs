﻿using Microsoft.AspNetCore.Mvc.Controllers;
using System.Diagnostics;
using System.Text;

namespace WebApi.Api.Middleware;

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

        var stopwatch = Stopwatch.StartNew();
        string controller = descriptor.ControllerName;
        string action = descriptor.ActionName;
        string path = context.Request.Path + context.Request.QueryString;

        string? requestBody = await TryReadRequestBodyAsync(context.Request);

        _logger.LogInformation(
            "{Method} {Path} ({Controller}/{Action}) | RequestBody: {Body}",
            context.Request.Method,
            path,
            controller,
            action,
            string.IsNullOrWhiteSpace(requestBody) ? "(empty)" : requestBody);

        Stream originalBody = context.Response.Body;
        using MemoryStream buffer = new();
        context.Response.Body = buffer;

        try
        {
            await _next(context);

            stopwatch.Stop();

            buffer.Seek(0, SeekOrigin.Begin);
            string responseBody = await ReadStreamAsync(buffer);
            buffer.Seek(0, SeekOrigin.Begin);

            int statusCode = context.Response.StatusCode;
            LogLevel level = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

            _logger.Log(
                level,
                "{StatusCode} {Method} {Path} ({Controller}/{Action}) | Elapsed: {Elapsed} ms\nResponseBody: {Body}",
                statusCode,
                context.Request.Method,
                path,
                controller,
                action,
                stopwatch.ElapsedMilliseconds,
                string.IsNullOrWhiteSpace(responseBody) ? "(no response body)" : responseBody);

            await buffer.CopyToAsync(originalBody);
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            context.Response.Body = originalBody;
            buffer.Dispose();
        }
    }

    private static async Task<string?> TryReadRequestBodyAsync(HttpRequest request)
    {
        if (request.ContentLength == 0)
        {
            return null;
        }

        request.EnableBuffering();

        using (StreamReader reader = new(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
        {
            string body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return body;
        }
    }

    private static async Task<string> ReadStreamAsync(Stream stream)
    {
        using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }
}