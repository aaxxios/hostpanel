/**
 * Tests for the knowledge-base category page: the heading and the articles of one category, and
 * its empty and loading states.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountPage, registerPageComponents, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import type { PublicPageSpies } from '~/tests/helpers/publicPage'
import KbArticleRow from '~/components/kb/ArticleRow.vue'

/** The mocked content endpoints, hoisted so the mocked module can reach them. */
const api = vi.hoisted(() => ({
  fetchAnnouncements: vi.fn(),
  fetchRecentAnnouncements: vi.fn(),
  fetchKbCategories: vi.fn(),
  fetchKbArticles: vi.fn(),
  fetchKbArticle: vi.fn(),
}))

vi.mock('~/composables/apis/useContentApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useContentApi: () => api,
}))

const { default: Page } = await import('~/pages/knowledgebase/category/[id].vue')

/** Spies of the stood-in runtime, renewed before each test. */
let spies: PublicPageSpies

describe('pages/knowledgebase/category/[id]', () => {
  beforeEach(() => {
    Object.values(api).forEach(fn => fn.mockReset())
    spies = stubPublicPageRuntime({ params: { id: '2' } })
    registerPageComponents({ KbArticleRow })
    api.fetchKbCategories.mockResolvedValue([{ id: 2, name: 'Billing', articlecount: 2 }])
  })

  it('shows the category name and how many articles it holds', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 1, title: 'Invoices' }, { id: 2, title: 'Refunds' }] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-category-heading"]').text()).toBe('Billing')
    expect(wrapper.find('[data-testid="kb-category-count"]').text()).toBe('2 kb.articles')
  })

  it('asks the API for the articles of this category', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [] })

    await mountPage(Page)

    expect(api.fetchKbArticles).toHaveBeenCalledWith(expect.objectContaining({ categoryid: '2' }))
  })

  it('lists the articles as rows', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 1, title: 'Invoices' }, { id: 2, title: 'Refunds' }] })

    const wrapper = await mountPage(Page)

    const rows = wrapper.find('[data-testid="kb-category-articles"]').findAll('[data-testid^="kb-article-"][href]')
    expect(rows).toHaveLength(2)
    expect(rows[0]?.attributes('href')).toBe('/en/knowledgebase/1')
  })

  it('falls back to the generic heading for a category the API does not list', async () => {
    api.fetchKbCategories.mockResolvedValue([])
    api.fetchKbArticles.mockResolvedValue({ items: [] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-category-heading"]').text()).toBe('kb.categories')
  })

  it('shows the empty state for a category without articles', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-category-empty"]').text()).toContain('kb.empty')
  })

  it('sets the page title from the category name', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [] })

    await mountPage(Page)

    expect(spies.useSeoMeta).toHaveBeenCalledWith({ title: 'Billing — kb.title' })
  })

  it('shows placeholders while the articles load', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [] })
    vi.stubGlobal('useAsyncData', async () => ({ pending: ref(true) }))

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-category-loading"]').findAll('[data-testid="ui-skeleton"]')).toHaveLength(6)
  })
})
