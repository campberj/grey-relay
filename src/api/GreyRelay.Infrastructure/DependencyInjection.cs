using GreyRelay.Application.Abstractions;
using GreyRelay.Application.Campaigns;
using GreyRelay.Application.Journal;
using GreyRelay.Infrastructure.Campaigns;
using GreyRelay.Infrastructure.Persistence;
using GreyRelay.Infrastructure.Persistence.Journal;
using GreyRelay.Infrastructure.Time;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GreyRelay.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            ForeignKeys = true,
            DefaultTimeout = 30,
        }.ToString();

        services.AddSingleton(new DatabaseOptions(databasePath));
        services.AddDbContext<GreyRelayDbContext>(options => options.UseSqlite(connectionString));
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IEventJournal, SqliteEventJournal>();
        services.AddSingleton<ICampaignRepository, InMemoryCampaignRepository>();
        return services;
    }
}
