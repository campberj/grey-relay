using GreyRelay.Application.Campaigns;
using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Application.Tests;

public sealed class ListCampaignsTests
{
    [Fact]
    public async Task Returns_empty_list_when_no_campaigns_exist()
    {
        var listCampaigns = new ListCampaigns(new StubCampaignRepository([]));

        var result = await listCampaigns.ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Projects_campaigns_to_summaries_sorted_by_title()
    {
        var zulu = Campaign.Create(new CampaignId(Guid.Parse("00000000-0000-4000-8000-00000000000a")), new CampaignTitle("Zulu"));
        var alpha = Campaign.Create(new CampaignId(Guid.Parse("00000000-0000-4000-8000-00000000000b")), new CampaignTitle("alpha"));
        alpha.Archive(expectedRevision: 0);
        var listCampaigns = new ListCampaigns(new StubCampaignRepository([zulu, alpha]));

        var result = await listCampaigns.ExecuteAsync(TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                new CampaignSummary(alpha.Id.Value, "alpha", "archived", 1),
                new CampaignSummary(zulu.Id.Value, "Zulu", "active", 0),
            ],
            result);
    }

    private sealed class StubCampaignRepository(IReadOnlyList<Campaign> campaigns) : ICampaignRepository
    {
        public Task<IReadOnlyList<Campaign>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult(campaigns);
    }
}
