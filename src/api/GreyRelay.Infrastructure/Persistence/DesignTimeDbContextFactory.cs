using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GreyRelay.Infrastructure.Persistence;

/// <summary>Used only by <c>dotnet ef</c> to build migrations; never at runtime.</summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GreyRelayDbContext>
{
    public GreyRelayDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<GreyRelayDbContext>().UseSqlite("Data Source=design-time.db").Options);
}
