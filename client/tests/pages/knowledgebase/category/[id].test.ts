/**
 * Tests for the knowledge-base category page: the slug resolves to its category, the heading and
 * the articles of that category, its SEO tags, its empty, failure and loading states, the 404 for an
 * unknown slug, and the retryable error (not a 404) when the categories cannot be read.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
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
    spies = stubPublicPageRuntime({ params: { id: 'billing' } })
    registerPageComponents({ KbArticleRow })
    api.fetchKbCategories.mockResolvedValue([{ name: 'Billing', articleCount: 2 }])
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

    expect(api.fetchKbArticles).toHaveBeenCalledWith(expect.objectContaining({ category: 'Billing' }))
  })

  it('lists the articles as rows', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 1, title: 'Invoices' }, { id: 2, title: 'Refunds' }] })

    const wrapper = await mountPage(Page)

    const rows = wrapper.find('[data-testid="kb-category-articles"]').findAll('[data-testid^="kb-article-"][href]')
    expect(rows).toHaveLength(2)
    expect(rows[0]?.attributes('href')).toBe('/en/knowledgebase/1')
  })

  it('resolves the slug whatever its case', async () => {
    spies = stubPublicPageRuntime({ params: { id: 'Billing-Payments' } })
    api.fetchKbCategories.mockResolvedValue([{ name: 'Billing & Payments', articleCount: 0 }])
    api.fetchKbArticles.mockResolvedValue({ items: [] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-category-heading"]').text()).toBe('Billing & Payments')
    expect(api.fetchKbArticles).toHaveBeenCalledWith(expect.objectContaining({ category: 'Billing & Payments' }))
  })

  it('raises a 404 for a slug no category has', async () => {
    api.fetchKbCategories.mockResolvedValue([])
    api.fetchKbArticles.mockResolvedValue({ items: [] })
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    const consoleWarn = vi.spyOn(console, 'warn').mockImplementation(() => undefined)
    const errorHandler = vi.fn()

    const wrapper = await mountPage(Page, { errorHandler })

    expect(spies.createError).toHaveBeenCalledWith(expect.objectContaining({ statusCode: 404 }))
    expect(errorHandler).toHaveBeenCalledWith(expect.objectContaining({ statusCode: 404 }), expect.anything(), expect.anything())
    // The setup threw, so nothing of a category renders (the static page frame does, as for any
    // async setup that rejects under a test Suspense; under Nuxt the error page replaces it).
    expect(wrapper.find('[data-testid="kb-category-heading"]').exists()).toBe(false)
    expect(api.fetchKbArticles).not.toHaveBeenCalled()
    consoleError.mockRestore()
    consoleWarn.mockRestore()
  })

  it('shows the category read failure with a retry instead of a 404, and reads again on retry', async () => {
    api.fetchKbCategories.mockRejectedValueOnce({ data: { detail: 'Content is unavailable.' } })
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 1, title: 'Invoices' }] })

    const wrapper = await mountPage(Page)

    expect(spies.createError).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="kb-category-error"]').text()).toContain('Content is unavailable.')
    expect(wrapper.find('[data-testid="kb-category-heading"]').exists()).toBe(false)
    expect(api.fetchKbArticles).not.toHaveBeenCalled()

    await wrapper.find('[data-testid="kb-category-retry"]').trigger('click')
    await flushPromises()

    expect(api.fetchKbCategories).toHaveBeenCalledTimes(2)
    expect(wrapper.find('[data-testid="kb-category-error"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="kb-category-heading"]').text()).toBe('Billing')
    expect(wrapper.find('[data-testid="kb-category-articles"]').text()).toContain('Invoices')
  })

  it('shows the 404 page when a retried category read no longer has the slug', async () => {
    api.fetchKbCategories.mockRejectedValueOnce({ data: { detail: 'Content is unavailable.' } })
    api.fetchKbCategories.mockResolvedValue([])
    const showError = vi.fn()
    vi.stubGlobal('showError', showError)

    const wrapper = await mountPage(Page)
    await wrapper.find('[data-testid="kb-category-retry"]').trigger('click')
    await flushPromises()

    expect(showError).toHaveBeenCalledWith(expect.objectContaining({ statusCode: 404 }))
    expect(api.fetchKbArticles).not.toHaveBeenCalled()
  })

  it('shows the read failure with a retry instead of the empty state, and reads again on retry', async () => {
    api.fetchKbArticles.mockRejectedValueOnce({ data: { detail: 'Content is unavailable.' } })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-category-error"]').text()).toContain('Content is unavailable.')
    expect(wrapper.find('[data-testid="kb-category-empty"]').exists()).toBe(false)

    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 1, title: 'Invoices' }] })
    await wrapper.find('[data-testid="kb-category-retry"]').trigger('click')
    await flushPromises()

    expect(api.fetchKbArticles).toHaveBeenCalledTimes(2)
    expect(wrapper.find('[data-testid="kb-category-error"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="kb-category-articles"]').text()).toContain('Invoices')
  })

  it('shows the empty state for a category without articles', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-category-empty"]').text()).toContain('kb.empty')
  })

  it('sets the SEO tags once, with the canonical path on the slug', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [] })

    await mountPage(Page)

    expect(spies.useSeo).toHaveBeenCalledTimes(1)
    expect(spies.useSeo).toHaveBeenCalledWith(expect.objectContaining({
      title: 'kb.categorySeoTitle',
      description: 'kb.categorySeoDescription',
      path: '/knowledgebase/category/billing',
    }))
    expect(spies.useSeoMeta).not.toHaveBeenCalled()
  })

  it('publishes the breadcrumb trail as structured data', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [] })

    await mountPage(Page)

    expect(spies.injectSchema).toHaveBeenCalledWith({
      '@type': 'BreadcrumbList',
      items: [
        { name: 'kb.title', url: '/en/knowledgebase' },
        { name: 'Billing', url: '/en/knowledgebase/category/billing' },
      ],
    })
  })

  it('shows placeholders while the articles load', async () => {
    api.fetchKbArticles.mockResolvedValue({ items: [] })
    vi.stubGlobal('useAsyncData', async (_key: unknown, handler: () => Promise<unknown>) => {
      await handler()
      return { pending: ref(true) }
    })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-category-loading"]').findAll('[data-testid="ui-skeleton"]')).toHaveLength(6)
  })
})
