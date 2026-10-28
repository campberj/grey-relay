using GreyRelay.Domain.Campaigns;
using GreyRelay.Domain.Common;

namespace GreyRelay.Domain.Tests;

public sealed class CampaignTests
{
    private static readonly CampaignId AshVeilId = new(Guid.Parse("4f2a1c1e-0000-4000-8000-000000000001"));
    private static readonly CampaignId BlankId = new(Guid.Parse("4f2a1c1e-0000-4000-8000-000000000002"));

    [Fact]
    public void Create_starts_active_at_revision_zero()
    {
        var campaign = Campaign.Create(AshVeilId, new CampaignTitle("Operation Ash Veil"));

        Assert.Equal(AshVeilId, campaign.Id);
        Assert.Equal("Operation Ash Veil", campaign.Title.Value);
        Assert.Equal(CampaignStatus.Active, campaign.Status);
        Assert.Equal(0, campaign.Revision);
    }

    [Fact]
    public void Archive_at_current_revision_archives_and_advances_revision()
    {
        var campaign = Campaign.Create(AshVeilId, new CampaignTitle("Operation Ash Veil"));

        campaign.Archive(expectedRevision: 0);

        Assert.Equal(CampaignStatus.Archived, campaign.Status);
        Assert.Equal(1, campaign.Revision);
    }

    [Fact]
    public void Archive_at_stale_revision_is_rejected_and_state_is_unchanged()
    {
        var campaign = Campaign.Create(AshVeilId, new CampaignTitle("Operation Ash Veil"));

        var conflict = Assert.Throws<RevisionConflictException>(() => campaign.Archive(expectedRevision: 3));

        Assert.Equal(3, conflict.ExpectedRevision);
        Assert.Equal(0, conflict.ActualRevision);
        Assert.Equal(CampaignStatus.Active, campaign.Status);
        Assert.Equal(0, campaign.Revision);
    }

    [Fact]
    public void Archive_twice_is_rejected()
    {
        var campaign = Campaign.Create(AshVeilId, new CampaignTitle("Operation Ash Veil"));
        campaign.Archive(expectedRevision: 0);

        Assert.Throws<CampaignAlreadyArchivedException>(() => campaign.Archive(expectedRevision: 1));
        Assert.Equal(1, campaign.Revision);
    }

    [Fact]
    public void Campaigns_are_independent_aggregates()
    {
        var ashVeil = Campaign.Create(AshVeilId, new CampaignTitle("Operation Ash Veil"));
        var blank = Campaign.Create(BlankId, new CampaignTitle("Untitled Campaign"));

        ashVeil.Archive(expectedRevision: 0);

        Assert.Equal(CampaignStatus.Active, blank.Status);
        Assert.Equal(0, blank.Revision);
        Assert.NotEqual(ashVeil.Id, blank.Id);
    }

    [Fact]
    public void Same_commands_on_same_inputs_produce_same_state()
    {
        static Campaign Replay()
        {
            var campaign = Campaign.Create(AshVeilId, new CampaignTitle("Operation Ash Veil"));
            campaign.Archive(expectedRevision: 0);
            return campaign;
        }

        var first = Replay();
        var second = Replay();

        Assert.Equal((first.Id, first.Title, first.Status, first.Revision), (second.Id, second.Title, second.Status, second.Revision));
    }
}
