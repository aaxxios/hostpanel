/**
 * Tests for the slides API composable: paths, verbs and bodies through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useSlidesApi } from '@/composables/apis/useSlidesApi'
import { json, problem, stubFetch, callOf } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useSlidesApi', () => {
  it('listSlides reads /admin/slides', async () => {
    const mock = stubFetch(json([{ id: 1 }]))

    expect(await useSlidesApi().listSlides()).toEqual([{ id: 1 }])
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/slides', method: 'GET' })
  })

  it('createSlide POSTs the payload and returns the id', async () => {
    const mock = stubFetch(json(7))

    expect(await useSlidesApi().createSlide({ iconName: 'x' } as never)).toBe(7)
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/slides', method: 'POST', body: { iconName: 'x' } })
  })

  it('updateSlide PUTs by id and deleteSlide DELETEs by id', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useSlidesApi()

    await api.updateSlide(2, { id: 2 } as never)
    await api.deleteSlide(2)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/slides/2', method: 'PUT', body: { id: 2 } })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/slides/2', method: 'DELETE' })
  })

  it('updateSlideOrder PUTs the pairs wrapped in items', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))

    await useSlidesApi().updateSlideOrder([{ id: 1, sortOrder: 0 }])
    expect(callOf(mock)).toMatchObject({
      url: '/api/admin/slides/order',
      method: 'PUT',
      body: { items: [{ id: 1, sortOrder: 0 }] },
    })
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Slide is locked.', 409))

    await expect(useSlidesApi().deleteSlide(1)).rejects.toBeTruthy()
  })
})
