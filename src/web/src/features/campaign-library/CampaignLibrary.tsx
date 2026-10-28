import type { CampaignSummary } from '../../api/campaignApi'
import { useCampaigns } from './useCampaigns'

export function CampaignLibrary() {
  const state = useCampaigns()

  return (
    <section aria-labelledby="campaign-library-heading">
      <h1 id="campaign-library-heading">Campaign Library</h1>
      {state.status === 'loading' && <p role="status">Loading campaigns…</p>}
      {state.status === 'error' && (
        <p role="alert" className="notice notice--error">
          Could not load campaigns: {state.message}
        </p>
      )}
      {state.status === 'loaded' && <CampaignList campaigns={state.campaigns} />}
    </section>
  )
}

function CampaignList({ campaigns }: { campaigns: CampaignSummary[] }) {
  if (campaigns.length === 0) {
    return (
      <p className="notice">
        No campaigns yet. Creating and importing campaigns is planned for milestone S1.
      </p>
    )
  }

  return (
    <ul className="campaign-list">
      {campaigns.map((campaign) => (
        <li key={campaign.id} className="campaign-list__item">
          <span className="campaign-list__title">{campaign.title}</span>
          <span className="badge">Status: {campaign.status}</span>
          <span className="campaign-list__meta">Revision {campaign.revision}</span>
        </li>
      ))}
    </ul>
  )
}
