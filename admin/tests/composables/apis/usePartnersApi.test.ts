/**
 * Tests for the partners API composable: paths, verbs and bodies through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { usePartnersApi } from '@/composables/apis/usePartnersApi'
import { callOf, json, problem, stubFetch } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('usePartnersApi', () => {
  it('list reads /admin/partners', async () => {
    const mock = stubFetch(json([{ id: 1 }]))

    expect(await usePartnersApi().list()).toEqual([{ id: 1 }])
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/partners', method: 'GET' })
  })

  it('create POSTs the payload and returns the id', async () => {
    const mock = stubFetch(json(7))

    expect(await usePartnersApi().create({ translations: { en: { name: 'Acme' } } } as never)).toBe(7)
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/partners', method: 'POST', body: { translations: { en: { name: 'Acme' } } } })
  })

  it('update PUTs by id and remove DELETEs by id', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = usePartnersApi()

    await api.update(2, { translations: { en: { name: 'Acme' } } } as never)
    await api.remove(2)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/partners/2', method: 'PUT', body: { translations: { en: { name: 'Acme' } } } })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/partners/2', method: 'DELETE' })
  })

  it('reorder PUTs the pairs wrapped in items', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))

    await usePartnersApi().reorder([{ id: 1, sortOrder: 0 }])
    expect(callOf(mock)).toMatchObject({
      url: '/api/admin/partners/order',
      method: 'PUT',
      body: { items: [{ id: 1, sortOrder: 0 }] },
    })
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Entry is locked.', 409))

    await expect(usePartnersApi().remove(1)).rejects.toBeTruthy()
  })
})
