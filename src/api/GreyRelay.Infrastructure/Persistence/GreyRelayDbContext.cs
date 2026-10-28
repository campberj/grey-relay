using GreyRelay.Infrastructure.Persistence.Journal;
using Microsoft.EntityFrameworkCore;

namespace GreyRelay.Infrastructure.Persistence;

/// <summary>Unit of work over the campaign database.</summary>
public sealed class GreyRelayDbContext(DbContextOptions<GreyRelayDbContext> options) : DbContext(options)
{
    internal DbSet<CampaignEventRecord> CampaignEvents => Set<CampaignEventRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GreyRelayDbContext).Assembly);
    }
}
