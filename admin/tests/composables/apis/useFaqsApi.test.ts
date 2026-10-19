/**
 * Tests for the FAQ API composable: paths, verbs and bodies through the real `useApi`
 * transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useFaqsApi } from '@/composables/apis/useFaqsApi'
import { FaqCategory } from '@/types/storefrontenums'
import { callOf, json, problem, stubFetch } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useFaqsApi', () => {
  it('list reads /admin/faqs and returns the entries as the API sent them', async () => {
    const mock = stubFetch(json([{ id: 1, category: FaqCategory.Products }]))

    expect(await useFaqsApi().list()).toEqual([{ id: 1, category: FaqCategory.Products }])
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/faqs', method: 'GET' })
  })

  it('create POSTs the payload and returns the id', async () => {
    const mock = stubFetch(json(7))

    expect(await useFaqsApi().create({ category: FaqCategory.General } as never)).toBe(7)
    expect(callOf(mock)).toMatchObject({ url: '/api/admin/faqs', method: 'POST', body: { category: FaqCategory.General } })
  })

  it('update PUTs by id and remove DELETEs by id', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useFaqsApi()

    await api.update(2, { category: FaqCategory.General } as never)
    await api.remove(2)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/faqs/2', method: 'PUT', body: { category: FaqCategory.General } })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/admin/faqs/2', method: 'DELETE' })
  })

  it('reorder PUTs the pairs wrapped in items', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))

    await useFaqsApi().reorder([{ id: 1, sortOrder: 0 }])
    expect(callOf(mock)).toMatchObject({
      url: '/api/admin/faqs/order',
      method: 'PUT',
      body: { items: [{ id: 1, sortOrder: 0 }] },
    })
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Entry is locked.', 409))

    await expect(useFaqsApi().remove(1)).rejects.toBeTruthy()
  })
})
