/**
 * Tests for the client summary store: the summary and the three recent tables.
 */

import { describe, expect, it } from 'vitest'
import { useClientSummaryStore } from '@/stores/clientSummary'
import { json, problem, stubFetch } from '../helpers/fetch'
import { installStoreHooks } from '../helpers/storeHooks'

describe('clientSummaryStore', () => {
  installStoreHooks()

  it('fetchAll loads the summary and the three recent tables', async () => {
    const mock = stubFetch(
      json({ invoiceCount: 2 }),
      json({ items: [{ id: 1 }], totalCount: 1 }),
      json({ items: [{ id: 2 }], totalCount: 1 }),
      json({ items: [{ id: 3 }], totalCount: 1 }),
    )
    const store = useClientSummaryStore()

    await store.fetchAll(7)
    expect(store.loading).toBe(false)
    expect(store.summary).toEqual({ invoiceCount: 2 })
    expect(store.services).toHaveLength(1)
    expect(store.domains).toHaveLength(1)
    expect(store.quotes).toHaveLength(1)
    expect(mock).toHaveBeenCalledTimes(4)
  })

  it('a failed summary shows its sentence, a failed recent table stays silent', async () => {
    const store = useClientSummaryStore()
    stubFetch(problem('Summary unavailable.', 500))
    await store.fetchAll(7)
    expect(store.error).toBe('Summary unavailable.')
    expect(store.services).toEqual([])
  })

  it('reset clears the loaded state', async () => {
    stubFetch(json({ x: 1 }))
    const store = useClientSummaryStore()
    await store.fetchAll(7)

    store.reset()
    expect(store.summary).toBeNull()
    expect(store.error).toBeNull()
  })
})
