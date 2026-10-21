/**
 * Tests for the locations API composable: paths, verbs and bodies through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useLocationsApi } from '@/composables/apis/useLocationsApi'
import { callOf, json, problem, stubFetch } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useLocationsApi', () => {
  it('list reads /admin/locations', async () => {
    const mock = stubFetch(json([{ id: 1 }]))

    expect(await useLocationsApi().list()).toEqual([{ id: 1 }])
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/locations', method: 'GET' })
  })

  it('create POSTs the payload and returns the id', async () => {
    const mock = stubFetch(json(7))

    expect(await useLocationsApi().create({ translations: { en: { name: 'Acme' } } } as never)).toBe(7)
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/locations', method: 'POST', body: { translations: { en: { name: 'Acme' } } } })
  })

  it('update PUTs by id and remove DELETEs by id', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useLocationsApi()

    await api.update(2, { translations: { en: { name: 'Acme' } } } as never)
    await api.remove(2)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/locations/2', method: 'PUT', body: { translations: { en: { name: 'Acme' } } } })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/locations/2', method: 'DELETE' })
  })

  it('reorder PUTs the pairs wrapped in items', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))

    await useLocationsApi().reorder([{ id: 1, sortOrder: 0 }])
    expect(callOf(mock)).toMatchObject({
      url: '/api/admin/locations/order',
      method: 'PUT',
      body: { items: [{ id: 1, sortOrder: 0 }] },
    })
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Entry is locked.', 409))

    await expect(useLocationsApi().remove(1)).rejects.toBeTruthy()
  })
})
