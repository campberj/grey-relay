using GreyRelay.Domain.Common;

namespace GreyRelay.Domain.Campaigns;

/// <summary>
/// Aggregate root for one independent campaign instance. Every committed change advances
/// <see cref="Revision"/>; commands carry the revision they were issued against so concurrent
/// edits are rejected rather than overwritten.
/// </summary>
public sealed class Campaign
{
    private Campaign(CampaignId id, CampaignTitle title, CampaignStatus status, long revision)
    {
        Id = id;
        Title = title;
        Status = status;
        Revision = revision;
    }

    public CampaignId Id { get; }

    public CampaignTitle Title { get; }

    public CampaignStatus Status { get; private set; }

    public long Revision { get; private set; }

    public static Campaign Create(CampaignId id, CampaignTitle title)
    {
        ArgumentNullException.ThrowIfNull(title);
        return new Campaign(id, title, CampaignStatus.Active, revision: 0);
    }

    public void Archive(long expectedRevision)
    {
        EnsureRevision(expectedRevision);
        if (Status == CampaignStatus.Archived)
        {
            throw new CampaignAlreadyArchivedException(Id);
        }

        Status = CampaignStatus.Archived;
        Revision++;
    }

    private void EnsureRevision(long expectedRevision)
    {
        if (expectedRevision != Revision)
        {
            throw new RevisionConflictException(expectedRevision, Revision);
        }
    }
}
