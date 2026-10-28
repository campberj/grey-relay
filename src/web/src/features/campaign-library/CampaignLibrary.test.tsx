import { screen } from '@testing-library/react'
import { failingCampaignApi, fakeCampaignApi } from '../../test/fakeCampaignApi'
import { renderWithProviders } from '../../test/renderWithProviders'
import { CampaignLibrary } from './CampaignLibrary'

describe('CampaignLibrary', () => {
  it('shows a loading status before campaigns arrive', () => {
    renderWithProviders(<CampaignLibrary />, {
      campaignApi: { listCampaigns: () => new Promise(() => undefined) },
    })

    expect(screen.getByRole('status')).toHaveTextContent('Loading campaigns')
  })

  it('shows an empty state when there are no campaigns', async () => {
    renderWithProviders(<CampaignLibrary />, { campaignApi: fakeCampaignApi([]) })

    expect(await screen.findByText(/No campaigns yet/)).toBeInTheDocument()
    expect(screen.queryByRole('list')).not.toBeInTheDocument()
  })

  it('lists campaigns with status as text, not colour alone', async () => {
    renderWithProviders(<CampaignLibrary />, {
      campaignApi: fakeCampaignApi([
        {
          id: '00000000-0000-4000-8000-000000000001',
          title: 'Test Campaign',
          status: 'active',
          revision: 0,
        },
      ]),
    })

    const item = await screen.findByRole('listitem')
    expect(item).toHaveTextContent('Test Campaign')
    expect(item).toHaveTextContent('Status: active')
    expect(item).toHaveTextContent('Revision 0')
  })

  it('reports load failures as an alert', async () => {
    renderWithProviders(<CampaignLibrary />, { campaignApi: failingCampaignApi('HTTP 500') })

    expect(await screen.findByRole('alert')).toHaveTextContent('Could not load campaigns: HTTP 500')
  })
})
