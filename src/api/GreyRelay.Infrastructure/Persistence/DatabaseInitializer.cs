using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GreyRelay.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    /// <summary>Creates the database directory if needed, applies migrations and enables WAL.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.GetRequiredService<DatabaseOptions>();
        var directory = Path.GetDirectoryName(options.DatabasePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GreyRelayDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;", cancellationToken).ConfigureAwait(false);
    }
}
