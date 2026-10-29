/**
 * Tests for the predefined replies store: loads fill state, category writes reload the list,
 * search returns its hits without touching the full list, a refusal puts the API's sentence into
 * `error`, and a refused save exposes the field messages. The requests go through the real
 * `useApi` transport.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { translate } from '@/lib/translate'
import { usePredefinedRepliesStore } from '@/stores/predefinedReplies'
import { json, problem, stubFetch, stubOffline, callOf, invalid } from '../helpers/fetch'

/** A valid reply body. */
const reply = { name: 'Greeting', content: 'Hello', categoryId: 2 }

describe('predefinedRepliesStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('fetchCategories and fetchReplies fill their lists', async () => {
    const store = usePredefinedRepliesStore()
    stubFetch(json([{ id: 1 }]))
    expect(await store.fetchCategories()).toBe(true)
    expect(store.categories).toEqual([{ id: 1 }])

    const mock = stubFetch(json([{ id: 2 }]))
    expect(await store.fetchReplies(2)).toBe(true)
    expect(callOf(mock).url).toBe('/api/predefined-replies?categoryId=2')
    expect(store.replies).toEqual([{ id: 2 }])
    expect(store.loading).toBe(false)
  })

  it('a refused load reports the API sentence, the no-answer sentence offline', async () => {
    const store = usePredefinedRepliesStore()
    stubFetch(problem('Not allowed.', 403))
    expect(await store.fetchReplies()).toBe(false)
    expect(store.error).toBe('Not allowed.')

    stubOffline()
    expect(await store.fetchCategories()).toBe(false)
    expect(store.error).toBe(translate('errors.noAnswer'))
  })

  it('createCategory returns the id and reloads the categories', async () => {
    const store = usePredefinedRepliesStore()
    const mock = stubFetch(json(6), json([{ id: 6, name: 'General' }]))

    expect(await store.createCategory('General')).toBe(6)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/predefined-replies/categories', method: 'POST', body: { name: 'General' } })
    expect(store.categories).toEqual([{ id: 6, name: 'General' }])
  })

  it('createCategory returns null with the field messages and does not reload', async () => {
    const store = usePredefinedRepliesStore()
    const mock = stubFetch(invalid('Check the form.', { Name: ['Name is required.'] }))

    expect(await store.createCategory('')).toBeNull()
    expect(store.fieldErrors).toEqual({ name: 'Name is required.' })
    expect(mock).toHaveBeenCalledTimes(1)
  })

  it('deleteCategory reloads on success and keeps the sentence on refusal', async () => {
    const store = usePredefinedRepliesStore()
    stubFetch(json({}), json([]))
    expect(await store.deleteCategory(6)).toBe(true)

    stubFetch(problem('Category is in use.', 409))
    expect(await store.deleteCategory(6)).toBe(false)
    expect(store.actionError).toBe('Category is in use.')
    expect(store.error).toBeNull()
  })

  it('searchReplies returns the hits and null with the sentence when refused', async () => {
    const store = usePredefinedRepliesStore()
    store.replies.push({ id: 9 } as never)
    stubFetch(json([{ id: 1 }]))
    expect(await store.searchReplies('hi')).toEqual([{ id: 1 }])
    expect(store.replies).toEqual([{ id: 9 }])

    stubOffline()
    expect(await store.searchReplies('hi')).toBeNull()
    expect(store.actionError).toBe(translate('errors.noAnswer'))
  })

  it('reply writes report the outcome and the field messages', async () => {
    const store = usePredefinedRepliesStore()
    const mock = stubFetch(json(3))
    expect(await store.createReply(reply)).toBe(3)
    expect(callOf(mock)).toMatchObject({ url: '/api/predefined-replies', method: 'POST', body: reply })

    stubFetch(invalid('Check the form.', { Content: ['Content is required.'] }))
    expect(await store.updateReply(3, { ...reply, content: '' })).toBe(false)
    expect(store.fieldErrors).toEqual({ content: 'Content is required.' })

    stubFetch(new Response(null, { status: 204 }))
    expect(await store.deleteReply(3)).toBe(true)
  })

  it('updateCategory renames and refreshes the categories', async () => {
    const fetchMock = stubFetch(json({}), json([{ id: 1, name: 'New' }]))
    const store = usePredefinedRepliesStore()

    expect(await store.updateCategory(1, 'New')).toBe(true)
    expect(fetchMock.mock.calls[0]![0]).toBe('/api/predefined-replies/categories/1')
    expect(store.categories).toEqual([{ id: 1, name: 'New' }])
  })

  it('searchReplies returns the hits without replacing the full list', async () => {
    const store = usePredefinedRepliesStore()
    store.replies.push({ id: 9 } as never)
    stubFetch(json([{ id: 1 }]))

    expect(await store.searchReplies('hi')).toEqual([{ id: 1 }])
    expect(store.replies).toEqual([{ id: 9 }])
  })

  it('searchReplies returns null and sets actionError when the search fails', async () => {
    stubOffline()
    const store = usePredefinedRepliesStore()

    expect(await store.searchReplies('hi')).toBeNull()
    expect(store.actionError).toBe(translate('errors.noAnswer'))
  })
})
