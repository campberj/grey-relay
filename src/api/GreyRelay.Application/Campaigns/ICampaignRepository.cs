using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Application.Campaigns;

/// <summary>Persistence port for campaign aggregates; implemented in Infrastructure.</summary>
public interface ICampaignRepository
{
    Task<IReadOnlyList<Campaign>> ListAsync(CancellationToken cancellationToken);
}
