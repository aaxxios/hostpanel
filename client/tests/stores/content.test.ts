/**
 * Tests for `stores/content.ts`.
 *
 * The store, not the announcement and knowledgebase pages, owns the lists, the loading flags and
 * the failure sentence, so these assertions pin the contract the pages render from.
 *
 * @module tests/stores/content.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

/** The mocked content endpoints, re-created for every test. */
const api = {
  fetchAnnouncements: vi.fn(),
  fetchRecentAnnouncements: vi.fn(),
  fetchKbCategories: vi.fn(),
  fetchKbArticles: vi.fn(),
  fetchKbArticle: vi.fn()
}

vi.mock('~/composables/apis/useContentApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useContentApi: () => api
}))

const { useContentStore } = await import('~/stores/content')

/** An API refusal as `apiErrorMessage` reads it. */
const refusal = { data: { detail: 'Content unavailable.' } }

describe('useContentStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    Object.values(api).forEach(fn => fn.mockReset())
  })

  it('keeps the announcements and finds one by its route id', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [{ id: 3, title: 'A', date: '2026-01-01' }, { id: '4', title: 'B', date: '2026-02-01' }] })
    const store = useContentStore()

    await store.loadAnnouncements()

    expect(store.announcements).toHaveLength(2)
    expect(store.announcementFor('4')).toMatchObject({ title: 'B' })
    expect(store.announcementFor('99')).toBeNull()
    expect(store.loading).toBe(false)
  })

  it('keeps the API sentence and an empty list when the announcements are refused', async () => {
    api.fetchAnnouncements.mockRejectedValue(refusal)
    const store = useContentStore()

    const list = await store.loadAnnouncements()

    expect(list).toEqual([])
    expect(store.error).toBe('Content unavailable.')
  })

  it('keeps the dashboard news', async () => {
    api.fetchRecentAnnouncements.mockResolvedValue([{ id: 1, title: 'News', date: '2026-03-01' }])
    const store = useContentStore()

    await store.loadRecentNews()

    expect(store.recentNews).toEqual([{ id: 1, title: 'News', date: '2026-03-01' }])
  })

  it('orders the popular articles by views, most viewed first', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 1, title: 'a', views: 2 }, { id: 2, title: 'b', views: 9 }, { id: 3, title: 'c' }] })
    const store = useContentStore()

    await store.loadPopularArticles()

    expect(api.fetchKbArticles).toHaveBeenCalledWith({ limitnum: 10 })
    expect(store.popularArticles.map(a => a.id)).toEqual([2, 1, 3])
  })

  it('keeps articles per category and names the category from the loaded list', async () => {
    api.fetchKbCategories.mockResolvedValue([{ id: 7, name: 'Billing' }])
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 1, title: 'a' }] })
    const store = useContentStore()

    await store.loadKbCategories()
    await store.loadCategoryArticles('7')

    expect(api.fetchKbArticles).toHaveBeenCalledWith({ categoryid: '7', limitnum: 100 })
    expect(store.articlesFor('7')).toHaveLength(1)
    expect(store.articlesFor('8')).toEqual([])
    expect(store.categoryName('7')).toBe('Billing')
    expect(store.categoryName('8')).toBeNull()
  })

  it('keeps one article in full, and null when it cannot be read', async () => {
    api.fetchKbArticle.mockResolvedValueOnce({ id: 5, title: 'T', body: '<p>x</p>' }).mockRejectedValueOnce(refusal)
    const store = useContentStore()

    expect(await store.loadArticle('5')).toMatchObject({ title: 'T' })
    expect(store.articleFor('5')).toMatchObject({ title: 'T' })

    expect(await store.loadArticle('6')).toBeNull()
    expect(store.articleFor('6')).toBeNull()
    expect(store.error).toBe('Content unavailable.')
  })

  it('searches the knowledgebase, flags the search while it runs and can clear it', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 1, title: 'match' }] })
    const store = useContentStore()

    const pending = store.searchArticles('vps')
    expect(store.searching).toBe(true)
    await pending

    expect(api.fetchKbArticles).toHaveBeenCalledWith({ search: 'vps', limitnum: 50 })
    expect(store.searchResults).toHaveLength(1)
    expect(store.searching).toBe(false)

    store.clearSearch()
    expect(store.searchResults).toEqual([])
  })
})
