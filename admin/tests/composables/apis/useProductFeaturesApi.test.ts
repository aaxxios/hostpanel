/**
 * Tests for the product features API composable: paths, verbs, query strings and bodies through
 * the real `useApi` transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useProductFeaturesApi } from '@/composables/apis/useProductFeaturesApi'
import { json, stubFetch, callOf } from '../../helpers/fetch'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useProductFeaturesApi', () => {
  it('listFeatures filters by product', async () => {
    const mock = stubFetch(json([{ id: 1 }]))

    expect(await useProductFeaturesApi().listFeatures(4)).toEqual([{ id: 1 }])
    expect(callOf(mock).url).toBe('/api/product-features?productId=4')
  })

  it('createFeature POSTs the line', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const payload = { productId: 4, label: 'Disk', value: '10 GB', sortOrder: 0 }

    await useProductFeaturesApi().createFeature(payload)
    expect(callOf(mock)).toMatchObject({ url: '/api/product-features', method: 'POST', body: payload })
  })

  it('updateFeature PUTs under the line id and deleteFeature DELETEs', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }))
    const api = useProductFeaturesApi()
    const payload = { id: 2, label: 'Disk', value: '20 GB', sortOrder: 1 }

    await api.updateFeature(payload)
    await api.deleteFeature(2)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/product-features/2', method: 'PUT', body: payload })
    expect(callOf(mock, 1)).toMatchObject({ url: '/api/product-features/2', method: 'DELETE' })
  })
})
