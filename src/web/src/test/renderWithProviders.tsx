import { render } from '@testing-library/react'
import type { ReactElement } from 'react'
import { MemoryRouter } from 'react-router'
import type { CampaignApi } from '../api/campaignApi'
import { CampaignApiContext } from '../api/CampaignApiContext'

interface Options {
  campaignApi: CampaignApi
  route?: string
}

export function renderWithProviders(ui: ReactElement, { campaignApi, route = '/' }: Options) {
  return render(
    <MemoryRouter initialEntries={[route]}>
      <CampaignApiContext value={campaignApi}>{ui}</CampaignApiContext>
    </MemoryRouter>,
  )
}
