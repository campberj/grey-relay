namespace GreyRelay.Application.Campaigns;

/// <summary>Use case: list campaigns for the Campaign Library.</summary>
public sealed class ListCampaigns(ICampaignRepository repository)
{
    public async Task<IReadOnlyList<CampaignSummary>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var campaigns = await repository.ListAsync(cancellationToken).ConfigureAwait(false);
        return campaigns
            .OrderBy(campaign => campaign.Title.Value, StringComparer.OrdinalIgnoreCase)
            .Select(CampaignSummary.From)
            .ToList();
    }
}
