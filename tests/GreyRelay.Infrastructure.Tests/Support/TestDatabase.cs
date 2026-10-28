using GreyRelay.Application.Abstractions;
using GreyRelay.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace GreyRelay.Infrastructure.Tests.Support;

/// <summary>
/// A real, migrated SQLite database file in a temporary directory. Each <see cref="StartProcess"/>
/// call builds an independent service provider, so disposing one and starting another simulates
/// an application restart against the same file.
/// </summary>
internal sealed class TestDatabase : IAsyncDisposable
{
    public static readonly DateTimeOffset RecordedAt = new(2026, 10, 28, 12, 0, 0, TimeSpan.Zero);

    private readonly string _directory;

    private TestDatabase(string directory)
    {
        _directory = directory;
        DatabasePath = Path.Combine(directory, "nested", "greyrelay.test.db");
    }

    public string DatabasePath { get; }

    public static async Task<TestDatabase> CreateAsync()
    {
        var database = new TestDatabase(Path.Combine(Path.GetTempPath(), "greyrelay-tests", Guid.NewGuid().ToString("N")));
        await using var services = database.StartProcess();
        await services.InitializeDatabaseAsync(CancellationToken.None);
        return database;
    }

    public ServiceProvider StartProcess()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(DatabasePath);
        services.AddSingleton<IClock>(new FixedClock(RecordedAt));
        return services.BuildServiceProvider();
    }

    public ValueTask DisposeAsync()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }

        return ValueTask.CompletedTask;
    }
}
