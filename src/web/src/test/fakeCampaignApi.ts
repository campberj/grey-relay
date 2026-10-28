import type { CampaignApi, CampaignSummary } from '../api/campaignApi'

export function fakeCampaignApi(campaigns: CampaignSummary[] = []): CampaignApi {
  return { listCampaigns: () => Promise.resolve(campaigns) }
}

export function failingCampaignApi(message: string): CampaignApi {
  return { listCampaigns: () => Promise.reject(new Error(message)) }
}
