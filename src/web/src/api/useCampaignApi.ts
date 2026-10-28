import { use } from 'react'
import type { CampaignApi } from './campaignApi'
import { CampaignApiContext } from './CampaignApiContext'

export function useCampaignApi(): CampaignApi {
  const api = use(CampaignApiContext)
  if (!api) {
    throw new Error('useCampaignApi must be used inside a CampaignApiContext provider')
  }
  return api
}
