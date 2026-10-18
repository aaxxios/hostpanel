/**
 * Tests for the servers store: loads, group mutations and the failure-as-state actions.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useServersStore } from '@/stores/servers'
import { json, problem, stubFetch, stubOffline } from '../helpers/fetch'
import { installStoreHooks } from '../helpers/storeHooks'

describe('serversStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('createServer refreshes the list and returns true', async () => {
    const mock = stubFetch(json({}), json([{ id: 1 }]))
    const store = useServersStore()

    expect(await store.createServer({} as never)).toBe(true)
    expect(mock).toHaveBeenCalledTimes(2)
    expect(store.servers).toEqual([{ id: 1 }])
  })

  it('a refused deleteServer returns false and does not refresh', async () => {
    const mock = stubFetch(problem('Server has accounts.', 409))
    const store = useServersStore()

    expect(await store.deleteServer(1)).toBe(false)
    expect(store.actionError).toBe('Server has accounts.')
    expect(mock).toHaveBeenCalledTimes(1)
  })

  it('testConnection returns the result, null when the request is refused', async () => {
    const store = useServersStore()
    stubFetch(json({ connected: true }), json([]))
    expect(await store.testConnection(1)).toEqual({ connected: true })

    stubOffline()
    expect(await store.testConnection(1)).toBeNull()
    expect(store.actionError).toBe('Could not reach the server. Check your connection and try again.')
  })

  it('fetchServers sets error from the API on refusal', async () => {
    stubFetch(problem('Forbidden.', 403))
    const store = useServersStore()

    await store.fetchServers()
    expect(store.error).toBe('Forbidden.')
    expect(store.loading).toBe(false)
  })

  it('group mutations refresh both lists', async () => {
    const mock = stubFetch(json({}), json([]), json([]))
    const store = useServersStore()

    expect(await store.createGroup({} as never)).toBe(true)
    expect(mock).toHaveBeenCalledTimes(3)
  })
})

describe('serversStore loads', () => {
  installStoreHooks()

  it('fetchGroups fills the groups and a refusal keeps the API sentence', async () => {
    const store = useServersStore()

    stubFetch(json([{ id: 1 }]))
    await store.fetchGroups()
    expect(store.groups).toEqual([{ id: 1 }])

    stubFetch(problem('Groups locked.', 423))
    await store.fetchGroups()
    expect(store.error).toBe('Groups locked.')
    expect(store.loading).toBe(false)
  })

  it('createGroup refreshes both lists', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }), json([]), json([]))
    const store = useServersStore()

    expect(await store.createGroup({} as never)).toBe(true)
    expect(mock).toHaveBeenCalledTimes(3)
  })

  it('testConnection returns the probe result and refreshes the servers', async () => {
    const mock = stubFetch(json({ success: true }), json([{ id: 1 }]))
    const store = useServersStore()

    expect(await store.testConnection(1)).toEqual({ success: true })
    expect(store.servers).toEqual([{ id: 1 }])
    expect(mock).toHaveBeenCalledTimes(2)
  })
})
