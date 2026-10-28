using GreyRelay.Domain.Campaigns;

namespace GreyRelay.Domain.Tests;

public sealed class CampaignValueObjectTests
{
    [Fact]
    public void CampaignId_rejects_empty_guid() =>
        Assert.Throws<ArgumentException>(() => new CampaignId(Guid.Empty));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CampaignTitle_rejects_blank(string title) =>
        Assert.Throws<ArgumentException>(() => new CampaignTitle(title));

    [Fact]
    public void CampaignTitle_trims_whitespace() =>
        Assert.Equal("Operation Ash Veil", new CampaignTitle("  Operation Ash Veil ").Value);
}
