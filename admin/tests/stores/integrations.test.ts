/**
 * Tests for the integrations store: list with category normalisation, detail, save-then-reread
 * and the connection test, through the real `useApi` transport. A refusal keeps the API's own
 * sentence; nothing throws.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useIntegrationsStore } from '@/stores/integrations'
import { json, problem, stubFetch, stubOffline } from '../helpers/fetch'

/** The sentence shown when nothing answered. */
const NO_ANSWER = 'Could not reach the server. Check your connection and try again.'

beforeEach(() => {
  setActivePinia(createPinia())
})

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('integrationsStore.fetchAll', () => {
  it('maps the backend category strings to the frontend keys', async () => {
    stubFetch(json([
      { slug: 'stripe', category: 'Payment Gateways' },
      { slug: 'x', category: 'Unknown Group' },
    ]))
    const store = useIntegrationsStore()

    await store.fetchAll()
    expect(store.integrations.map(i => i.category)).toEqual(['payments', 'Unknown Group'])
    expect(store.loading).toBe(false)
  })

  it('keeps the API sentence on refusal', async () => {
    stubFetch(problem('Integrations unavailable.', 503))
    const store = useIntegrationsStore()

    await store.fetchAll()
    expect(store.error).toBe('Integrations unavailable.')
    expect(store.integrations).toEqual([])
  })
})

describe('integrationsStore.fetchOne', () => {
  it('loads the detail', async () => {
    stubFetch(json({ slug: 'stripe' }))
    const store = useIntegrationsStore()

    await store.fetchOne('stripe')
    expect(store.current).toEqual({ slug: 'stripe' })
  })

  it('empties current and keeps the no-answer sentence offline', async () => {
    stubOffline()
    const store = useIntegrationsStore()
    store.current = { slug: 'old' } as never

    await store.fetchOne('stripe')
    expect(store.current).toBeNull()
    expect(store.error).toBe(NO_ANSWER)
  })
})

describe('integrationsStore.saveConfig', () => {
  it('re-reads the detail after the 204 instead of blanking the page', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }), json({ slug: 'stripe', enabled: true }))
    const store = useIntegrationsStore()

    await store.saveConfig('stripe', { enabled: true, config: {} } as never)
    expect(mock).toHaveBeenCalledTimes(2)
    expect(store.current).toEqual({ slug: 'stripe', enabled: true })
  })

  it('keeps the API sentence and does not re-read on refusal', async () => {
    const mock = stubFetch(problem('Required field missing.', 422))
    const store = useIntegrationsStore()

    await store.saveConfig('stripe', {} as never)
    expect(store.error).toBe('Required field missing.')
    expect(mock).toHaveBeenCalledTimes(1)
  })
})

describe('integrationsStore.testConnection', () => {
  it('keeps the result with a duration and mirrors the last tested time', async () => {
    stubFetch(json({ success: true, message: 'ok', testedAt: '2026-01-01T00:00:00Z' }))
    const store = useIntegrationsStore()
    store.current = { slug: 'stripe', lastTestedAt: null } as unknown as NonNullable<typeof store.current>

    await store.testConnection('stripe')
    expect(store.testResult?.success).toBe(true)
    expect(store.testResult?.durationMs).toBeTypeOf('number')
    expect(store.current?.lastTestedAt).toBe('2026-01-01T00:00:00Z')
    expect(store.testing).toBe(false)
  })

  it('reports a refused test as an unsuccessful result carrying the API sentence', async () => {
    stubFetch(problem('Credentials rejected.', 502))
    const store = useIntegrationsStore()

    await store.testConnection('stripe')
    expect(store.testResult).toMatchObject({ success: false, message: 'Credentials rejected.' })
    expect(store.testing).toBe(false)
  })

  it('reports an unreachable server with the no-answer sentence', async () => {
    stubOffline()
    const store = useIntegrationsStore()

    await store.testConnection('stripe')
    expect(store.testResult).toMatchObject({ success: false, message: NO_ANSWER })
  })
})
