import { createContext } from 'react'
import type { CampaignApi } from './campaignApi'

/** Injection point for the campaign API; tests provide a fake implementation. */
export const CampaignApiContext = createContext<CampaignApi | null>(null)
