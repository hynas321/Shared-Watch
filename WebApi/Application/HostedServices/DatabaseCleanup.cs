using Microsoft.EntityFrameworkCore;

namespace WebApi.Application.HostedServices;

public class DatabaseCleanup : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseCleanup> _logger;

    public DatabaseCleanup(IServiceProvider serviceProvider, ILogger<DatabaseCleanup> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ClearAllTables(context);
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void ClearAllTables(AppDbContext context)
    {
        try
        {
            var entityTypes = context.Model.GetEntityTypes();

            foreach (var entityType in entityTypes)
            {
                var clrType = entityType.ClrType;
                var method = typeof(DbContext).GetMethod(nameof(DbContext.Set), []).MakeGenericMethod(clrType);
                var dbSet = method.Invoke(context, null);

                if (dbSet != null)
                {
                    var removeRangeMethod = dbSet.GetType().GetMethod("RemoveRange", [typeof(IEnumerable<>).MakeGenericType(clrType)]);
                    removeRangeMethod?.Invoke(dbSet, [dbSet]);
                }
            }

            context.SaveChanges();
            _logger.LogInformation("Database cleanup finished");
        }
        catch
        {
            throw new Exception("Database tables could not be cleared on startup. Check database connection and context.");
        }
    }
}