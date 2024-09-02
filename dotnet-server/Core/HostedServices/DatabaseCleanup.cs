using Microsoft.EntityFrameworkCore;

namespace DotnetServer.Core.Services;

public class DatabaseCleanup : IHostedService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseCleanup> _logger;

    public DatabaseCleanup(AppDbContext context, ILogger<DatabaseCleanup> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ClearAllTables();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void ClearAllTables()
    {
        var entityTypes = _context.Model.GetEntityTypes();

        foreach (var entityType in entityTypes)
        {
            var clrType = entityType.ClrType;
            var method = typeof(DbContext).GetMethod(nameof(DbContext.Set), Array.Empty<Type>()).MakeGenericMethod(clrType);
            var dbSet = method.Invoke(_context, null);

            if (dbSet != null)
            {
                var removeRangeMethod = dbSet.GetType().GetMethod("RemoveRange", new[] { typeof(IEnumerable<>).MakeGenericType(clrType) });
                removeRangeMethod.Invoke(dbSet, new object[] { dbSet });
            }
        }

        _context.SaveChanges();
        _logger.LogInformation("Database cleanup finished");
    }
}