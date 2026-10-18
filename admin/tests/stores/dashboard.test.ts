/**
 * Tests for the dashboard store: data on success, the API sentence on refusal, the offline fallback.
 */

import { describe, expect, it } from 'vitest'
import { useDashboardStore } from '@/stores/dashboard'
import { json, problem, stubFetch, stubOffline } from '../helpers/fetch'
import { installStoreHooks } from '../helpers/storeHooks'

describe('dashboardStore', () => {
  installStoreHooks()

  it('fetchStats stores the statistics', async () => {
    stubFetch(json({ totalClients: 7 }))
    const store = useDashboardStore()

    await store.fetchStats()
    expect(store.stats).toEqual({ totalClients: 7 })
    expect(store.error).toBeNull()
    expect(store.loading).toBe(false)
  })

  it('fetchStats shows the API sentence on refusal', async () => {
    stubFetch(problem('No access.', 403))
    const store = useDashboardStore()

    await store.fetchStats()
    expect(store.stats).toBeNull()
    expect(store.error).toBe('No access.')
    expect(store.loading).toBe(false)
  })

  it('fetchStats falls back when nothing answered', async () => {
    stubOffline()
    const store = useDashboardStore()

    await store.fetchStats()
    expect(store.error).toBe('Could not reach the server. Check your connection and try again.')
  })
})
