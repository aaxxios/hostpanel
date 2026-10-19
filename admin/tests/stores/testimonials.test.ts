/**
 * Tests for the testimonials store: the list load, the mutations and the image upload report success
 * as a value and keep a refusal in state (`error`, `actionError`, `uploadError`) instead of throwing.
 */

import { describe, expect, it } from 'vitest'
import { useTestimonialsStore } from '@/stores/testimonials'
import { json, problem, stubFetch, stubOffline } from '../helpers/fetch'
import { installStoreHooks, NO_ANSWER } from '../helpers/storeHooks'

/** A file standing in for the picked image. */
const file = new File(['x'], 'a.png', { type: 'image/png' })

describe('useTestimonialsStore', () => {
  installStoreHooks()

  it('fetchAll fills the list from /admin/testimonials', async () => {
    const mock = stubFetch(json([{ id: 1, sortOrder: 0 }]))
    const store = useTestimonialsStore()

    await store.fetchAll()

    expect(store.items).toEqual([{ id: 1, sortOrder: 0 }])
    expect(store.loading).toBe(false)
    expect(mock.mock.calls[0]?.[0]).toBe('/api/admin/testimonials')
  })

  it('a refused load keeps the API sentence in error', async () => {
    stubFetch(problem('Locked.', 423))
    const store = useTestimonialsStore()

    await store.fetchAll()

    expect(store.error).toBe('Locked.')
  })

  it('create returns the id and refreshes the list', async () => {
    const mock = stubFetch(json(5), json([{ id: 5, sortOrder: 0 }]))
    const store = useTestimonialsStore()

    expect(await store.create({} as never)).toBe(5)
    expect(mock).toHaveBeenCalledTimes(2)
    expect(store.items).toEqual([{ id: 5, sortOrder: 0 }])
  })

  it('update, remove and updateOrder return false with the API sentence', async () => {
    stubFetch(problem('Entry is locked.', 409))
    const store = useTestimonialsStore()

    expect(await store.update(1, {} as never)).toBe(false)
    expect(store.actionError).toBe('Entry is locked.')
    expect(await store.remove(1)).toBe(false)
    expect(await store.updateOrder([{ id: 1, sortOrder: 0 }])).toBe(false)
  })

  it('create returns null offline with the fallback sentence', async () => {
    stubOffline()
    const store = useTestimonialsStore()

    expect(await store.create({} as never)).toBeNull()
    expect(store.actionError).toBe(NO_ANSWER)
  })

  it('fetchOne serves from the cache and loads the list when it is empty', async () => {
    const mock = stubFetch(json([{ id: 3, sortOrder: 0 }]))
    const store = useTestimonialsStore()

    expect(await store.fetchOne(3)).toEqual({ id: 3, sortOrder: 0 })
    expect(await store.fetchOne(3)).toEqual({ id: 3, sortOrder: 0 })
    expect(mock).toHaveBeenCalledTimes(1)
  })

  it('uploadImage returns the stored URL, or null with the sentence in uploadError', async () => {
    stubFetch(json({ url: '/img/a.png' }), problem('The file is too large.', 413))
    const store = useTestimonialsStore()

    expect(await store.uploadImage(file)).toBe('/img/a.png')
    expect(await store.uploadImage(file)).toBeNull()
    expect(store.uploadError).toBe('The file is too large.')
  })
})
