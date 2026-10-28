import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { fakeCampaignApi } from '../test/fakeCampaignApi'
import { renderWithProviders } from '../test/renderWithProviders'
import { App } from './App'

describe('App', () => {
  it('opens on the Campaign Library', async () => {
    renderWithProviders(<App />, { campaignApi: fakeCampaignApi() })

    expect(
      await screen.findByRole('heading', { level: 1, name: 'Campaign Library' }),
    ).toBeInTheDocument()
  })

  it('navigates to the Operations Center placeholder by keyboard', async () => {
    const user = userEvent.setup()
    renderWithProviders(<App />, { campaignApi: fakeCampaignApi() })

    const link = screen.getByRole('link', { name: 'Operations Center' })
    link.focus()
    await user.keyboard('{Enter}')

    expect(screen.getByRole('heading', { level: 1, name: 'Operations Center' })).toBeInTheDocument()
    expect(screen.getByText(/No campaign is open/)).toBeInTheDocument()
  })

  it('shows a not-found page for unknown routes', () => {
    renderWithProviders(<App />, { campaignApi: fakeCampaignApi(), route: '/does-not-exist' })

    expect(screen.getByRole('heading', { level: 1, name: 'Page not found' })).toBeInTheDocument()
  })
})
