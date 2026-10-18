/**
 * Tests for the TLD configs store: loads and mutations reporting the outcome as a value.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useTldConfigsStore } from '@/stores/tldConfigs'
import { json, problem, stubFetch, stubOffline } from '../helpers/fetch'
import { installStoreHooks } from '../helpers/storeHooks'

describe('tldConfigsStore mutations', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('create returns the id and refreshes the list', async () => {
    const fetchMock = stubFetch(json(8), json([{ id: 8 }]))
    const store = useTldConfigsStore()

    expect(await store.create({} as never)).toBe(8)
    expect(fetchMock).toHaveBeenCalledTimes(2)
    expect(store.configs).toEqual([{ id: 8 }])
  })

  it('update, remove and syncPrices return false with the API sentence', async () => {
    stubFetch(problem('TLD is in use.', 409))
    const store = useTldConfigsStore()

    expect(await store.update(1, {} as never)).toBe(false)
    expect(store.actionError).toBe('TLD is in use.')
    expect(await store.remove(1)).toBe(false)
    expect(await store.syncPrices()).toBe(false)
  })

  it('importFromProvider returns the counts, null offline', async () => {
    const store = useTldConfigsStore()
    stubFetch(json({ imported: 2, updated: 1 }), json([]))
    expect(await store.importFromProvider('NameAm')).toEqual({ imported: 2, updated: 1 })

    stubOffline()
    expect(await store.importFromProvider('NameAm')).toBeNull()
    expect(store.actionError).toBe('Could not reach the server. Check your connection and try again.')
  })
})

describe('tldConfigsStore loads', () => {
  installStoreHooks()

  it('fetchAll fills the list and fetchById returns the config', async () => {
    const store = useTldConfigsStore()

    stubFetch(json([{ id: 1 }]))
    await store.fetchAll()
    expect(store.configs).toEqual([{ id: 1 }])

    stubFetch(json({ id: 1, tld: '.com' }))
    expect(await store.fetchById(1)).toEqual({ id: 1, tld: '.com' })
  })

  it('fetchById returns undefined and keeps the API sentence on refusal', async () => {
    stubFetch(problem('No such TLD.', 404))
    const store = useTldConfigsStore()

    expect(await store.fetchById(1)).toBeUndefined()
    expect(store.error).toBe('No such TLD.')
  })

  it('importFromProvider returns the counts, or null with actionError', async () => {
    const store = useTldConfigsStore()

    stubFetch(json({ imported: 3 }), json([]))
    expect(await store.importFromProvider('Namecheap')).toEqual({ imported: 3 })

    stubFetch(problem('Provider down.', 502))
    expect(await store.importFromProvider('Namecheap')).toBeNull()
    expect(store.actionError).toBe('Provider down.')
  })
})
