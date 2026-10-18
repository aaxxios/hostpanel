/**
 * Tests for useClientOptions: the list starts empty, load fills it from the clients endpoint with
 * the display name composed, and a refused load leaves it empty.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useClientOptions } from '@/composables/useClientOptions'
import { json, problem, stubFetch } from '../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useClientOptions', () => {
  it('starts empty and fills from the clients endpoint', async () => {
    setActivePinia(createPinia())
    stubFetch(
      json({
        items: [{ id: 7, firstName: 'Ada', lastName: 'Lovelace', companyName: 'Analytical', email: 'ada@example.com', status: 'Active' }],
        totalCount: 1,
      }),
    )
    const { clients, load } = useClientOptions()
    expect(clients.value).toEqual([])

    await load()

    expect(clients.value).toEqual([
      { id: 7, name: 'Ada Lovelace (Analytical)', email: 'ada@example.com', status: 'Active' },
    ])
  })

  it('stays empty when the endpoint refuses', async () => {
    setActivePinia(createPinia())
    stubFetch(problem('Clients are unavailable.', 403))
    const { clients, load } = useClientOptions()

    await load()

    expect(clients.value).toEqual([])
  })
})
