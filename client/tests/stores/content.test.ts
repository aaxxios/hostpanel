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
  fetchAnnouncement: vi.fn(),
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

  it('keeps the announcement list', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [{ id: 3, title: 'A', content: '', publishedAt: '2026-01-01' }, { id: 4, title: 'B', content: '', publishedAt: '2026-02-01' }] })
    const store = useContentStore()

    await store.loadAnnouncements()

    expect(store.announcements).toHaveLength(2)
    expect(store.loading).toBe(false)
  })

  it('reads one announcement by id from the detail endpoint, whatever page the list is on', async () => {
    api.fetchAnnouncement.mockResolvedValue({ id: 104, title: 'Old news', content: '<p>x</p>', publishedAt: '2024-01-01' })
    const store = useContentStore()

    const loaded = await store.loadAnnouncement('104')

    expect(api.fetchAnnouncement).toHaveBeenCalledWith('104')
    expect(api.fetchAnnouncements).not.toHaveBeenCalled()
    expect(loaded).toMatchObject({ title: 'Old news' })
    expect(store.announcementFor('104')).toMatchObject({ title: 'Old news' })
    expect(store.announcementFor('99')).toBeNull()
  })

  it('keeps the API sentence and no announcement when the detail is refused', async () => {
    api.fetchAnnouncement.mockRejectedValue(refusal)
    const store = useContentStore()

    expect(await store.loadAnnouncement('9')).toBeNull()
    expect(store.announcementFor('9')).toBeNull()
    expect(store.error).toBe('Content unavailable.')
  })

  it('keeps the failure status of a refused read and clears it on the next good one', async () => {
    api.fetchAnnouncement.mockRejectedValueOnce({ statusCode: 503, data: { detail: 'Unavailable.' } })
    api.fetchAnnouncement.mockResolvedValueOnce({ id: 9, title: 'Back', content: '', publishedAt: '2026-03-01' })
    const store = useContentStore()

    await store.loadAnnouncement('9')
    expect(store.errorStatus).toBe(503)

    await store.loadAnnouncement('9')
    expect(store.errorStatus).toBeNull()
  })

  it('records 502 when a read got no answer', async () => {
    api.fetchAnnouncement.mockRejectedValue(new TypeError('fetch failed'))
    const store = useContentStore()

    await store.loadAnnouncement('9')

    expect(store.errorStatus).toBe(502)
  })

  it('keeps the API sentence and an empty list when the announcements are refused', async () => {
    api.fetchAnnouncements.mockRejectedValue(refusal)
    const store = useContentStore()

    const list = await store.loadAnnouncements()

    expect(list).toEqual([])
    expect(store.error).toBe('Content unavailable.')
  })

  it('keeps the dashboard news', async () => {
    api.fetchRecentAnnouncements.mockResolvedValue({ items: [{ id: 1, title: 'News', content: '', publishedAt: '2026-03-01' }] })
    const store = useContentStore()

    await store.loadRecentNews()

    expect(store.recentNews).toEqual([{ id: 1, title: 'News', content: '', publishedAt: '2026-03-01' }])
  })

  it('keeps the first page of articles in the order the backend sent them', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 2, title: 'b', excerpt: '', category: 'x' }, { id: 1, title: 'a', excerpt: '', category: 'x' }, { id: 3, title: 'c', excerpt: '', category: 'x' }] })
    const store = useContentStore()

    await store.loadLandingArticles()

    expect(api.fetchKbArticles).toHaveBeenCalledWith({ pageSize: 10 })
    expect(store.landingArticles.map(a => a.id)).toEqual([2, 1, 3])
  })

  it('keeps articles per category and names the category from the loaded list', async () => {
    api.fetchKbCategories.mockResolvedValue([{ name: 'Billing', articleCount: 1 }])
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 1, title: 'a' }] })
    const store = useContentStore()

    await store.loadKbCategories()
    await store.loadCategoryArticles('Billing')

    expect(api.fetchKbArticles).toHaveBeenCalledWith({ category: 'Billing', pageSize: 100 })
    expect(store.articlesFor('Billing')).toHaveLength(1)
    expect(store.articlesFor('Domains')).toEqual([])
    expect(store.categoryBySlug('billing')?.name).toBe('Billing')
    expect(store.categoryBySlug('domains')).toBeNull()
  })

  it('finds a category by its slug whatever the case', async () => {
    api.fetchKbCategories.mockResolvedValue([{ name: 'Billing & Payments', articleCount: 3 }])
    const store = useContentStore()

    await store.loadKbCategories()

    expect(store.categoryBySlug('Billing-Payments')?.name).toBe('Billing & Payments')
    expect(store.categoryBySlug('billing-payments')?.articleCount).toBe(3)
    expect(store.categoryBySlug('billing')).toBeNull()
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

    expect(api.fetchKbArticles).toHaveBeenCalledWith({ search: 'vps', pageSize: 50 })
    expect(store.searchResults).toHaveLength(1)
    expect(store.searching).toBe(false)

    store.clearSearch()
    expect(store.searchResults).toEqual([])
  })
})
