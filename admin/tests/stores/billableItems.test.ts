/**
 * Tests for the billable items store: create refreshes the list, a failure lands in `error`.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useBillableItemsStore } from '@/stores/billableItems'
import { json, stubFetch, stubOffline } from '../helpers/fetch'

describe('billableItemsStore.create', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('returns true and refreshes the list', async () => {
    stubFetch(json({}), json({ items: [{ id: 1 }], totalCount: 1 }))
    const store = useBillableItemsStore()

    expect(await store.create({ clientId: 1, description: 'x', amount: 1, currency: 'USD', type: 'OneTime' })).toBe(true)
    expect(store.items).toEqual([{ id: 1 }])
  })

  it('returns false and sets error when the call fails', async () => {
    stubOffline()
    const store = useBillableItemsStore()

    expect(await store.create({ clientId: 1, description: 'x', amount: 1, currency: 'USD', type: 'OneTime' })).toBe(false)
    expect(store.error).toBe('Could not reach the server. Check your connection and try again.')
  })
})
