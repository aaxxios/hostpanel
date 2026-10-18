/**
 * Tests for the client activity log store: filters reach the request, a refusal lands in the store.
 */

import { describe, expect, it } from 'vitest'
import { useClientLogStore } from '@/stores/clientLog'
import { json, problem, stubFetch, callOf } from '../helpers/fetch'
import { installStoreHooks } from '../helpers/storeHooks'

describe('clientLogStore', () => {
  installStoreHooks()

  it('fetchLogs sends only the filters that are set', async () => {
    const mock = stubFetch(json({ items: [{ id: 1 }], totalCount: 1 }))
    const store = useClientLogStore()

    await store.fetchLogs(7, 1, 25, { description: 'login', ipAddress: '' })
    expect(store.logs).toEqual([{ id: 1 }])
    expect(callOf(mock).url).toBe('/api/clients/7/activity-logs?page=1&pageSize=25&description=login')
  })

  it('fetchLogs shows the API sentence on refusal and reset clears the state', async () => {
    const store = useClientLogStore()
    stubFetch(problem('Log is locked.', 403))
    await store.fetchLogs(7, 1, 25)
    expect(store.error).toBe('Log is locked.')

    store.reset()
    expect(store.error).toBeNull()
    expect(store.logs).toEqual([])
  })
})
