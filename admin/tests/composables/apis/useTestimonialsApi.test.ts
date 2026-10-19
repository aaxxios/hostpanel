/**
 * Tests for the testimonials API composable: paths, verbs and bodies through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useTestimonialsApi } from '@/composables/apis/useTestimonialsApi'
import { callOf, json, problem, stubFetch } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useTestimonialsApi', () => {
  it('list reads /admin/testimonials', async () => {
    const mock = stubFetch(json([{ id: 1 }]))

    expect(await useTestimonialsApi().list()).toEqual([{ id: 1 }])
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/testimonials', method: 'GET' })
  })

  it('create POSTs the payload and returns the id', async () => {
    const mock = stubFetch(json(7))

    expect(await useTestimonialsApi().create({ name: 'Ann', isActive: true } as never)).toBe(7)
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/testimonials', method: 'POST', body: { name: 'Ann', isActive: true } })
  })

  it('update PUTs by id and remove DELETEs by id', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useTestimonialsApi()

    await api.update(2, { name: 'Ann', isActive: true } as never)
    await api.remove(2)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/testimonials/2', method: 'PUT', body: { name: 'Ann', isActive: true } })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/testimonials/2', method: 'DELETE' })
  })

  it('reorder PUTs the pairs wrapped in items', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))

    await useTestimonialsApi().reorder([{ id: 1, sortOrder: 0 }])
    expect(callOf(mock)).toMatchObject({
      url: '/api/admin/testimonials/order',
      method: 'PUT',
      body: { items: [{ id: 1, sortOrder: 0 }] },
    })
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Entry is locked.', 409))

    await expect(useTestimonialsApi().remove(1)).rejects.toBeTruthy()
  })
})
