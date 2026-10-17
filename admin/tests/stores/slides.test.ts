/**
 * Tests for the slides store: the image upload and the slide mutations report success as a
 * value and keep a refusal in state (`uploadError`, `actionError`) instead of throwing.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useSlidesStore } from '@/stores/slides'
import { json, problem, stubFetch, stubOffline } from '../helpers/fetch'
import { installStoreHooks } from '../helpers/storeHooks'

/** A file standing in for the picked image. */
const file = new File(['x'], 'slide.png', { type: 'image/png' })

describe('slidesStore.uploadImage', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('returns the stored image url', async () => {
    stubFetch(json({ url: '/img/a.png' }))

    expect(await useSlidesStore().uploadImage(file)).toBe('/img/a.png')
  })

  it('returns null and keeps the detail sentence of a problem+json refusal', async () => {
    stubFetch(problem('The file is too large.', 413))
    const store = useSlidesStore()

    expect(await store.uploadImage(file)).toBeNull()
    expect(store.uploadError).toBe('The file is too large.')
  })

  it('keeps the fallback when the body carried no sentence', async () => {
    stubFetch(new Response('<html>', { status: 502 }))
    const store = useSlidesStore()

    expect(await store.uploadImage(file)).toBeNull()
    expect(store.uploadError).toBe('Could not reach the server. Check your connection and try again.')
  })
})

describe('slidesStore mutations', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('createSlide returns the id and refreshes the list', async () => {
    const fetchMock = stubFetch(json(5), json([{ id: 5 }]))
    const store = useSlidesStore()

    expect(await store.createSlide({} as never)).toBe(5)
    expect(fetchMock).toHaveBeenCalledTimes(2)
    expect(store.slides).toEqual([{ id: 5 }])
  })

  it('updateSlide, deleteSlide and updateOrder return false with the API sentence', async () => {
    stubFetch(problem('Slide is locked.', 409))
    const store = useSlidesStore()

    expect(await store.updateSlide(1, {} as never)).toBe(false)
    expect(store.actionError).toBe('Slide is locked.')
    expect(await store.deleteSlide(1)).toBe(false)
    expect(await store.updateOrder([{ id: 1, sortOrder: 0 }])).toBe(false)
  })

  it('keeps the per-field messages of a refused create and clears them on the next attempt', async () => {
    stubFetch(
      json({ type: 'VALIDATION_ERROR', title: 'Validation failed', status: 400, detail: 'One or more validation errors occurred.', errors: { iconName: ['Icon is required.'], imageUrl: ['Image is required.'] } }, 400),
      json(7),
      json([]),
    )
    const store = useSlidesStore()

    expect(await store.createSlide({} as never)).toBeNull()
    expect(store.fieldErrors).toEqual({ iconName: 'Icon is required.', imageUrl: 'Image is required.' })

    expect(await store.createSlide({} as never)).toBe(7)
    expect(store.fieldErrors).toEqual({})
  })

  it('createSlide returns null offline with the fallback sentence', async () => {
    stubOffline()
    const store = useSlidesStore()

    expect(await store.createSlide({} as never)).toBeNull()
    expect(store.actionError).toBe('Could not reach the server. Check your connection and try again.')
  })
})

describe('slidesStore.fetchSlides and fetchSlide', () => {
  installStoreHooks()

  it('fetchSlide serves from the cache without a request', async () => {
    const mock = stubFetch(json([]))
    const store = useSlidesStore()
    store.slides.push({ id: 3 } as never)

    expect(await store.fetchSlide(3)).toEqual({ id: 3 })
    expect(mock).not.toHaveBeenCalled()
  })

  it('fetchSlide loads the list first when the cache is empty', async () => {
    stubFetch(json([{ id: 3 }]))
    const store = useSlidesStore()

    expect(await store.fetchSlide(3)).toEqual({ id: 3 })
  })

  it('a refused load keeps the API sentence in error', async () => {
    stubFetch(problem('Slides locked.', 423))
    const store = useSlidesStore()

    await store.fetchSlides()
    expect(store.error).toBe('Slides locked.')
    expect(store.loading).toBe(false)
  })
})
