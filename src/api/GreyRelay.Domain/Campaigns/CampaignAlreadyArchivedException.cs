using GreyRelay.Domain.Common;

namespace GreyRelay.Domain.Campaigns;

public sealed class CampaignAlreadyArchivedException : DomainException
{
    public CampaignAlreadyArchivedException(CampaignId campaignId)
        : base($"Campaign {campaignId} is already archived.")
    {
        CampaignId = campaignId;
    }

    public CampaignId CampaignId { get; }
}
