using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Application.Campaigns;

/// <summary>Player-visible read model for the Campaign Library. Never expose aggregates directly.</summary>
public sealed record CampaignSummary(Guid Id, string Title, string Status, long Revision)
{
    public static CampaignSummary From(Campaign campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        return new CampaignSummary(
            campaign.Id.Value,
            campaign.Title.Value,
            campaign.Status.ToString().ToLowerInvariant(),
            campaign.Revision);
    }
}
