/**
 * Tests for the knowledge base store: loads fill state, category writes reload the list, a
 * refusal puts the API's sentence into `error`, and a refused save exposes the field messages.
 * The requests go through the real `useApi` transport.
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { translate } from '@/lib/translate'
import { useKnowledgebaseStore } from '@/stores/knowledgebase'
import { json, problem, stubFetch, stubOffline, callOf, invalid } from '../helpers/fetch'

/** A valid category body. */
const category = { name: 'Billing', description: 'Invoices', isHidden: false }

describe('knowledgebaseStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('fetchCategories and fetchArticles fill their lists', async () => {
    const store = useKnowledgebaseStore()
    stubFetch(json([{ id: 1 }]))
    expect(await store.fetchCategories()).toBe(true)
    expect(store.categories).toEqual([{ id: 1 }])

    stubFetch(json([{ id: 2 }]))
    expect(await store.fetchArticles()).toBe(true)
    expect(store.articles).toEqual([{ id: 2 }])
    expect(store.loading).toBe(false)
  })

  it('a refused load reports the API sentence, the no-answer sentence offline', async () => {
    const store = useKnowledgebaseStore()
    stubFetch(problem('Not allowed.', 403))
    expect(await store.fetchArticles()).toBe(false)
    expect(store.error).toBe('Not allowed.')

    stubOffline()
    expect(await store.fetchCategories()).toBe(false)
    expect(store.error).toBe(translate('errors.noAnswer'))
  })

  it('createCategory returns the id and reloads the categories', async () => {
    const store = useKnowledgebaseStore()
    const mock = stubFetch(json(5), json([{ id: 5, name: 'Billing' }]))

    expect(await store.createCategory(category)).toBe(5)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/knowledgebase/categories', method: 'POST', body: category })
    expect(callOf(mock, 1).url).toBe('/api/admin/knowledgebase/categories')
    expect(store.categories).toEqual([{ id: 5, name: 'Billing' }])
  })

  it('createCategory returns null with the field messages and does not reload', async () => {
    const store = useKnowledgebaseStore()
    const mock = stubFetch(invalid('Check the form.', { Name: ['Name is required.'] }))

    expect(await store.createCategory({ ...category, name: '' })).toBeNull()
    expect(store.fieldErrors).toEqual({ name: 'Name is required.' })
    expect(mock).toHaveBeenCalledTimes(1)
  })

  it('updateCategory and deleteCategory reload on success only', async () => {
    const store = useKnowledgebaseStore()
    const mock = stubFetch(json({}), json([]))
    expect(await store.updateCategory(5, category)).toBe(true)
    expect(await store.deleteCategory(5)).toBe(true)
    expect(callOf(mock, 0)).toMatchObject({ url: '/api/admin/knowledgebase/categories/5', method: 'PUT' })
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/admin/knowledgebase/categories/5', method: 'DELETE' })

    const refused = stubFetch(problem('Category is in use.', 409))
    expect(await store.deleteCategory(5)).toBe(false)
    expect(store.error).toBe('Category is in use.')
    expect(refused).toHaveBeenCalledTimes(1)
  })

  it('createArticle returns the id and deleteArticle reports the outcome', async () => {
    const store = useKnowledgebaseStore()
    const article = { title: 'How to pay', content: 'Use a card', category: 'Billing', isPublished: true }

    stubFetch(json(11))
    expect(await store.createArticle(article)).toBe(11)

    stubFetch(problem('No such article.', 404))
    expect(await store.deleteArticle(11)).toBe(false)
    expect(store.error).toBe('No such article.')
  })
})
