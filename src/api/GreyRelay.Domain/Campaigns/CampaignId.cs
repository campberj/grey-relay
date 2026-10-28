namespace GreyRelay.Domain.Campaigns;

/// <summary>Stable identity of one campaign instance.</summary>
public readonly record struct CampaignId
{
    public CampaignId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Campaign id must not be empty.", nameof(value));
        }

        Value = value;
    }

    public Guid Value { get; }

    public override string ToString() => Value.ToString();
}
