using GreyRelay.Application.Campaigns;
using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Infrastructure.Campaigns;

/// <summary>
/// S0 placeholder adapter holding no campaigns. Replaced by durable SQLite storage in S1
/// (ADR 0001); nothing here is persisted.
/// </summary>
public sealed class InMemoryCampaignRepository : ICampaignRepository
{
    public Task<IReadOnlyList<Campaign>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Campaign>>([]);
}
