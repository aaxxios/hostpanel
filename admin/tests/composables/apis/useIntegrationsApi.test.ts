/**
 * Tests for the integrations API composable: paths, verbs and bodies through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useIntegrationsApi } from '@/composables/apis/useIntegrationsApi'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useIntegrationsApi', () => {
  it('listIntegrations and getIntegration read their endpoints', async () => {
    const mock = stubFetch(json([]))
    const api = useIntegrationsApi()

    await api.listIntegrations()
    await api.getIntegration('stripe')
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/integrations', method: 'GET' })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/integrations/stripe', method: 'GET' })
  })

  it('saveIntegration PUTs the payload', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))

    await useIntegrationsApi().saveIntegration('stripe', { enabled: true, config: {} } as never)
    expect(callOf(mock)).toMatchObject({
      url: '/api/admin/integrations/stripe',
      method: 'PUT',
      body: { enabled: true, config: {} },
    })
  })

  it('testIntegration POSTs to test and returns the result', async () => {
    const mock = stubFetch(json({ success: true }))

    expect(await useIntegrationsApi().testIntegration('stripe')).toEqual({ success: true })
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/integrations/stripe/test', method: 'POST' })
  })

  it('getCwpServerInfo and getCwp7ServerInfo read their server-info endpoints', async () => {
    const mock = stubFetch(json({}))
    const api = useIntegrationsApi()

    await api.getCwpServerInfo()
    await api.getCwp7ServerInfo()
    expect(callOf(mock, 0).url).toBe('/api/admin/integrations/cwp/server-info')
    expect(callOf(mock, 1).url).toBe('/api/admin/integrations/cwp7/server-info')
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Required field missing.', 422))

    await expect(useIntegrationsApi().saveIntegration('x', {} as never)).rejects.toBeTruthy()
  })
})
