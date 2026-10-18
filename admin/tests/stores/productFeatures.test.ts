/**
 * Tests for the product features store: loads return data or null with the API's sentence, a
 * refused add or save exposes the field messages, and the saved text is trimmed. The requests go
 * through the real `useApi` transport.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { translate } from '@/lib/translate'
import { useProductFeaturesStore } from '@/stores/productFeatures'
import { json, problem, stubFetch, stubOffline, callOf, invalid } from '../helpers/fetch'

describe('productFeaturesStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('fetchFeatures returns the lines of the product', async () => {
    const mock = stubFetch(json([{ id: 1, label: 'Disk', value: '10 GB' }]))
    const store = useProductFeaturesStore()

    expect(await store.fetchFeatures(4)).toHaveLength(1)
    expect(callOf(mock).url).toBe('/api/product-features?productId=4')
  })

  it('fetchFeatures returns null with the API sentence, the no-answer sentence offline', async () => {
    const store = useProductFeaturesStore()
    stubFetch(problem('No such product.', 404))
    expect(await store.fetchFeatures(4)).toBeNull()
    expect(store.error).toBe('No such product.')

    stubOffline()
    expect(await store.fetchFeatures(4)).toBeNull()
    expect(store.error).toBe(translate('errors.noAnswer'))
  })

  it('addFeature posts the line with its position', async () => {
    const mock = stubFetch(json({}))
    const store = useProductFeaturesStore()

    expect(await store.addFeature(4, 'Disk', '10 GB', 2)).toBe(true)
    expect(callOf(mock)).toMatchObject({
      url: '/api/product-features',
      method: 'POST',
      body: { productId: 4, label: 'Disk', value: '10 GB', sortOrder: 2 },
    })
  })

  it('a refused add exposes the field messages and the next success clears them', async () => {
    const store = useProductFeaturesStore()
    stubFetch(invalid('Check the form.', { Label: ['Label is required.'] }))

    expect(await store.addFeature(4, '', '10 GB', 0)).toBe(false)
    expect(store.error).toBe('Check the form.')
    expect(store.fieldErrors).toEqual({ label: 'Label is required.' })

    stubFetch(json({}))
    expect(await store.addFeature(4, 'Disk', '10 GB', 0)).toBe(true)
    expect(store.fieldErrors).toEqual({})
  })

  it('saveFeature trims the text and removeFeature reports refusals', async () => {
    const mock = stubFetch(json({}))
    const store = useProductFeaturesStore()

    expect(await store.saveFeature({ id: 1, productId: 4, label: ' Disk ', value: ' 10 GB ', sortOrder: 0 })).toBe(true)
    expect(callOf(mock)).toMatchObject({
      url: '/api/product-features/1',
      method: 'PUT',
      body: { id: 1, label: 'Disk', value: '10 GB', sortOrder: 0 },
    })

    stubFetch(problem('Line is locked.'))
    expect(await store.removeFeature(1)).toBe(false)
    expect(store.error).toBe('Line is locked.')
  })
})
