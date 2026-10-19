/**
 * Tests for the shared storefront CRUD state: the load, the refusal sentences, the per-field
 * messages and the cache lookup, through the real transport with `fetch` stubbed.
 */

import { afterEach, describe, expect, it, vi } from 'vitest'
import { useStorefrontApi } from '@/composables/apis/useStorefrontApi'
import { ApiPath } from '@/lib/apiPaths'
import { useStorefrontCrud } from '@/composables/useStorefrontCrud'
import { invalid, json, problem, stubFetch } from '../helpers/fetch'

/** An entry as the tests use it. */
interface Widget {
  /** Entry ID. */
  id: number
  /** Display order. */
  sortOrder: number
}

/**
 * Builds the CRUD state over the `/admin/faqs` endpoint set.
 *
 * @returns The state and actions.
 */
const makeCrud = () => useStorefrontCrud<Widget, { name: string }>(useStorefrontApi<Widget, { name: string }>(ApiPath.AdminFaqs))

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('useStorefrontCrud', () => {
  it('fetchAll loads the list and clears the loading flag', async () => {
    stubFetch(json([{ id: 1, sortOrder: 0 }]))
    const crud = makeCrud()

    await crud.fetchAll()

    expect(crud.items.value).toEqual([{ id: 1, sortOrder: 0 }])
    expect(crud.loading.value).toBe(false)
    expect(crud.error.value).toBeNull()
  })

  it('keeps the per-field messages of a refused create and clears them on the next attempt', async () => {
    stubFetch(invalid('Check the form.', { name: ['Name is required.'] }), json(2), json([]))
    const crud = makeCrud()

    expect(await crud.create({ name: '' })).toBeNull()
    expect(crud.fieldErrors.value).toEqual({ name: 'Name is required.' })
    expect(crud.actionError.value).toBe('Check the form.')

    expect(await crud.create({ name: 'x' })).toBe(2)
    expect(crud.fieldErrors.value).toEqual({})
    expect(crud.actionError.value).toBeNull()
  })

  it('refreshes the list after a successful update, delete and reorder', async () => {
    const mock = stubFetch(new Response(null, { status: 204 }), json([{ id: 9, sortOrder: 0 }]))
    const crud = makeCrud()

    expect(await crud.update(1, { name: 'x' })).toBe(true)
    expect(await crud.remove(1)).toBe(true)
    expect(await crud.updateOrder([{ id: 1, sortOrder: 0 }])).toBe(true)

    expect(mock).toHaveBeenCalledTimes(6)
    expect(crud.items.value).toEqual([{ id: 9, sortOrder: 0 }])
  })

  it('a refused reorder reloads the list so the server order is restored', async () => {
    const mock = stubFetch(problem('Locked.', 409), json([{ id: 1, sortOrder: 0 }]))
    const crud = makeCrud()

    expect(await crud.updateOrder([{ id: 1, sortOrder: 5 }])).toBe(false)

    expect(mock).toHaveBeenCalledTimes(2)
    expect(crud.items.value).toEqual([{ id: 1, sortOrder: 0 }])
    expect(crud.actionError.value).toBe('Locked.')
  })

  it('resetErrors clears every sentence and field message', async () => {
    stubFetch(invalid('Check the form.', { name: ['Required.'] }), problem('Not an image.', 415), json([]))
    const crud = makeCrud()
    await crud.create({ name: '' })
    await crud.uploadImage(new File(['x'], 'a.txt'))

    crud.resetErrors()

    expect(crud.actionError.value).toBeNull()
    expect(crud.uploadError.value).toBeNull()
    expect(crud.fieldErrors.value).toEqual({})
  })

  it('fetchOne answers from the cache and undefined for an unknown id', async () => {
    const mock = stubFetch(json([{ id: 3, sortOrder: 0 }]))
    const crud = makeCrud()

    expect(await crud.fetchOne(3)).toEqual({ id: 3, sortOrder: 0 })
    expect(await crud.fetchOne(8)).toBeUndefined()
    expect(mock).toHaveBeenCalledTimes(1)
  })

  it('uploadImage keeps a refusal in uploadError', async () => {
    stubFetch(problem('Not an image.', 415))
    const crud = makeCrud()

    expect(await crud.uploadImage(new File(['x'], 'a.txt'))).toBeNull()
    expect(crud.uploadError.value).toBe('Not an image.')
  })
})
