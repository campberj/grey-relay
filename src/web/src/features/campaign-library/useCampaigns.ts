import { useEffect, useState } from 'react'
import type { CampaignSummary } from '../../api/campaignApi'
import { useCampaignApi } from '../../api/useCampaignApi'

export type CampaignsState =
  | { status: 'loading' }
  | { status: 'loaded'; campaigns: CampaignSummary[] }
  | { status: 'error'; message: string }

export function useCampaigns(): CampaignsState {
  const api = useCampaignApi()
  const [state, setState] = useState<CampaignsState>({ status: 'loading' })

  useEffect(() => {
    const controller = new AbortController()
    api.listCampaigns(controller.signal).then(
      (campaigns) => {
        if (!controller.signal.aborted) {
          setState({ status: 'loaded', campaigns })
        }
      },
      (error: unknown) => {
        if (!controller.signal.aborted) {
          setState({
            status: 'error',
            message: error instanceof Error ? error.message : 'Unknown error',
          })
        }
      },
    )
    return () => {
      controller.abort()
    }
  }, [api])

  return state
}
