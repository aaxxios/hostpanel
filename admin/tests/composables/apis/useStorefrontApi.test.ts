/**
 * Tests for the shared storefront endpoint set: the route root decides every path.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useStorefrontApi } from '@/composables/apis/useStorefrontApi'
import { ApiPath } from '@/lib/apiPaths'
import { callOf, json, problem, stubFetch } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useStorefrontApi', () => {
  it('builds every path from the route root', async () => {
    const mock = stubFetch(json([]), json(4), new Response(null, { status: 204 }))
    const api = useStorefrontApi<{ id: number }, { name: string }>(ApiPath.AdminFaqs)

    await api.list()
    await api.create({ name: 'a' })
    await api.update(4, { name: 'b' })
    await api.remove(4)
    await api.reorder([{ id: 4, sortOrder: 1 }])

    expect([0, 1, 2, 3, 4].map((index) => `${callOf(mock, index).method} ${callOf(mock, index).url}`)).toEqual([
      'GET /api/admin/faqs',
      'POST /api/admin/faqs',
      'PUT /api/admin/faqs/4',
      'DELETE /api/admin/faqs/4',
      'PUT /api/admin/faqs/order',
    ])
    expect(callOf(mock, 1).body).toEqual({ name: 'a' })
    expect(callOf(mock, 4).body).toEqual({ items: [{ id: 4, sortOrder: 1 }] })
  })

  it('rejects when the API refuses', async () => {
    stubFetch(problem('Nope.', 403))

    await expect(useStorefrontApi(ApiPath.AdminFaqs).list()).rejects.toBeTruthy()
  })
})
