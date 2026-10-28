import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router'
import { CampaignApiContext } from './api/CampaignApiContext'
import { createHttpCampaignApi } from './api/httpCampaignApi'
import { App } from './app/App'
import './styles.css'

const rootElement = document.getElementById('root')
if (!rootElement) {
  throw new Error('Root element #root not found')
}

// Composition root: concrete adapters are chosen here and injected through context.
const campaignApi = createHttpCampaignApi()

createRoot(rootElement).render(
  <StrictMode>
    <BrowserRouter>
      <CampaignApiContext value={campaignApi}>
        <App />
      </CampaignApiContext>
    </BrowserRouter>
  </StrictMode>,
)
