using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace GreyRelay.Api.Tests;

/// <summary>Hosts the API in-process against a throwaway database file, never the developer's.</summary>
public sealed class GreyRelayApiFactory : WebApplicationFactory<Program>
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "greyrelay-api-tests", Guid.NewGuid().ToString("N"));

    public string DatabasePath => Path.Combine(_directory, "greyrelay.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("Storage:DatabasePath", DatabasePath);

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}
