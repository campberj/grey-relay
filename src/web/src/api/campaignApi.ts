import type { components } from './schema'

export type CampaignSummary = components['schemas']['CampaignSummary']

/** Port for campaign data. Components depend on this abstraction, never on fetch directly. */
export interface CampaignApi {
  listCampaigns(signal?: AbortSignal): Promise<CampaignSummary[]>
}

export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}
