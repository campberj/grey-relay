import { ApiError, type CampaignApi, type CampaignSummary } from './campaignApi'

type Fetch = typeof fetch

/** HTTP adapter for {@link CampaignApi}, talking to the local GREY RELAY API. */
export function createHttpCampaignApi(
  fetchFn: Fetch = (input, init) => fetch(input, init),
): CampaignApi {
  return {
    async listCampaigns(signal) {
      const response = await fetchFn('/api/campaigns', {
        headers: { Accept: 'application/json' },
        signal: signal ?? null,
      })
      if (!response.ok) {
        throw new ApiError(
          response.status,
          `Failed to load campaigns (HTTP ${String(response.status)})`,
        )
      }
      return (await response.json()) as CampaignSummary[]
    },
  }
}
