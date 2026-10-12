using WebApi.Middleware;
using WebApi.SignalR;

namespace WebApi.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseSharedWatchPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<RequestResponseLoggingMiddleware>();
        app.UseCors("AllowReactApplication");

        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapHub<AppHub>("/Hub/Room");
        app.MapControllers();
        return app;
    }
}
