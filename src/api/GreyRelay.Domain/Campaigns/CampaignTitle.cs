namespace GreyRelay.Domain.Campaigns;

/// <summary>Display title of a campaign; never blank.</summary>
public sealed record CampaignTitle
{
    public CampaignTitle(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
