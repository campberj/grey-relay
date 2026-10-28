import { ApiError } from './campaignApi'
import { createHttpCampaignApi } from './httpCampaignApi'

describe('createHttpCampaignApi', () => {
  it('requests the campaign list from the local API', async () => {
    const fetchFn = vi.fn<typeof fetch>().mockResolvedValue(Response.json([]))
    const api = createHttpCampaignApi(fetchFn)

    await expect(api.listCampaigns()).resolves.toEqual([])

    expect(fetchFn).toHaveBeenCalledWith(
      '/api/campaigns',
      expect.objectContaining({ signal: null }),
    )
  })

  it('throws an ApiError carrying the HTTP status on failure', async () => {
    const fetchFn = vi.fn<typeof fetch>().mockResolvedValue(new Response(null, { status: 503 }))
    const api = createHttpCampaignApi(fetchFn)

    const failure = api.listCampaigns()

    await expect(failure).rejects.toBeInstanceOf(ApiError)
    await expect(failure).rejects.toMatchObject({ status: 503 })
  })
})
