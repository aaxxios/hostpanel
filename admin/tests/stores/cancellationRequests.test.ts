/**
 * Tests for the cancellation requests store: the paged load and the delete.
 */

import { describe, expect, it } from 'vitest'
import { useCancellationRequestsStore } from '@/stores/cancellationRequests'
import { json, problem, stubFetch, callOf } from '../helpers/fetch'
import { installStoreHooks } from '../helpers/storeHooks'

describe('cancellationRequestsStore', () => {
  installStoreHooks()

  it('fetchAll fills the page from the store paging', async () => {
    const mock = stubFetch(json({ items: [{ id: 1 }], totalCount: 5 }))
    const store = useCancellationRequestsStore()

    await store.fetchAll()
    expect(store.cancellations).toEqual([{ id: 1 }])
    expect(store.totalCount).toBe(5)
    expect(callOf(mock).url).toBe('/api/admin/cancellation-requests?page=1&pageSize=20')
  })

  it('deleteRequest reloads on success and keeps the sentence on refusal', async () => {
    const store = useCancellationRequestsStore()
    const mock = stubFetch(new Response(null, { status: 204 }), json({ items: [], totalCount: 0 }))
    expect(await store.deleteRequest(3)).toBe(true)
    expect(mock).toHaveBeenCalledTimes(2)

    stubFetch(problem('Request not found.', 404))
    expect(await store.deleteRequest(3)).toBe(false)
    expect(store.error).toBe('Request not found.')
    expect(store.loading).toBe(false)
  })
})
