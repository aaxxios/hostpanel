/**
 * Tests for the knowledge-base landing page: categories and the landing article list read through the
 * content store, and the search that swaps them for results and writes `?q=`.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { ref } from 'vue'
import { mountPage, registerPageComponents, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
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

const { default: Page } = await import('~/pages/knowledgebase/index.vue')

/** Spy for the router the page writes the query to. */
const router = { push: vi.fn(), replace: vi.fn(), back: vi.fn() }

/** Two categories, one holding a single article. */
const CATEGORIES = [
  { name: 'Billing', articleCount: 4 },
  { name: 'Domains', articleCount: 1 },
]

/** Articles the landing list and the search both return. */
const ARTICLES = {
  items: [
    { id: 31, title: 'Getting started', excerpt: 'Start here', category: 'General' },
    { id: 30, title: 'Next steps', excerpt: '', category: 'General' },
  ],
}

/**
 * Answers the content endpoints with the fixtures above.
 *
 * @param articles - What the article endpoint returns (landing list and search alike).
 */
const answerWith = (articles: { items: unknown[] } = ARTICLES): void => {
  api.fetchKbCategories.mockResolvedValue(CATEGORIES)
  api.fetchKbArticles.mockResolvedValue(articles)
}

describe('pages/knowledgebase/index', () => {
  beforeEach(() => {
    router.replace.mockReset()
    Object.values(api).forEach(fn => fn.mockReset())
    stubPublicPageRuntime()
    vi.stubGlobal('useRouter', () => router)
    registerPageComponents({ KbArticleRow })
  })

  // One test swaps `useAsyncData` for a read that stays pending; undo it so the next test gets the
  // setup file's stand-in again (it reinstalls the auto-imports before every test).
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('shows the hero and the search box with its placeholder', async () => {
    answerWith()

    const wrapper = await mountPage(Page)

    expect(wrapper.find('h1').text()).toBe('kb.title')
    expect(wrapper.find('[data-testid="kb-search-input"]').attributes('placeholder')).toBe('kb.searchPlaceholder')
  })

  it('links each category to its page with an article count', async () => {
    answerWith()

    const wrapper = await mountPage(Page)

    const billing = wrapper.find('[data-testid="kb-category-card-billing"]')
    expect(billing.attributes('href')).toBe('/en/knowledgebase/category/billing')
    expect(billing.text()).toContain('Billing')
    expect(billing.text()).toContain('4 kb.articles')
    expect(wrapper.find('[data-testid="kb-category-card-domains"]').text()).toContain('1 kb.article')
  })

  it('lists the first articles in the order the backend sent them', async () => {
    answerWith()

    const wrapper = await mountPage(Page)

    const rows = wrapper.findAll('[data-testid="kb-articles"] [data-testid^="kb-article-"][href]')
    expect(rows.map(row => row.attributes('data-testid'))).toEqual(['kb-article-31', 'kb-article-30'])
  })

  it('says so when there are no categories and no articles', async () => {
    api.fetchKbCategories.mockResolvedValue([])
    api.fetchKbArticles.mockResolvedValue({ items: [] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-categories"]').text()).toContain('kb.empty')
    expect(wrapper.find('[data-testid="kb-articles"]').text()).toContain('kb.empty')
  })

  it('runs a search, writes it to the URL and shows the results instead of the landing view', async () => {
    answerWith()
    const wrapper = await mountPage(Page)
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 40, title: 'DNS basics' }] })

    await wrapper.find('[data-testid="kb-search-input"]').setValue('dns')
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(router.replace).toHaveBeenCalledWith({ query: { q: 'dns' } })
    expect(api.fetchKbArticles).toHaveBeenLastCalledWith(expect.objectContaining({ search: 'dns' }))
    expect(wrapper.find('[data-testid="kb-search-count"]').text()).toBe('1 kb.articles')
    expect(wrapper.find('[data-testid="kb-search-results"] [data-testid="kb-article-40"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="kb-categories"]').exists()).toBe(false)
  })

  it('shows the empty-result state for a search that finds nothing', async () => {
    answerWith()
    const wrapper = await mountPage(Page)
    api.fetchKbArticles.mockResolvedValue({ items: [] })

    await wrapper.find('[data-testid="kb-search-input"]').setValue('zzz')
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(wrapper.find('[data-testid="kb-search-empty"]').text()).toContain('kb.noResults')
  })

  it('returns to the landing view and clears the query', async () => {
    answerWith()
    const wrapper = await mountPage(Page)
    await wrapper.find('[data-testid="kb-search-input"]').setValue('dns')
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    await wrapper.find('[data-testid="kb-clear-search"]').trigger('click')

    expect(router.replace).toHaveBeenLastCalledWith({ query: {} })
    expect(wrapper.find('[data-testid="kb-categories"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="kb-search-input"]').element).toHaveProperty('value', '')
  })

  it('treats a blank search as clearing it', async () => {
    answerWith()
    const wrapper = await mountPage(Page)
    api.fetchKbArticles.mockClear()

    await wrapper.find('[data-testid="kb-search-input"]').setValue('   ')
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(api.fetchKbArticles).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="kb-categories"]').exists()).toBe(true)
  })

  it('repeats the search of a shared ?q= link on load', async () => {
    answerWith()
    stubPublicPageRuntime({ query: { q: 'ssl' } })
    vi.stubGlobal('useRouter', () => router)
    api.fetchKbArticles.mockResolvedValue({ items: [{ id: 41, title: 'Install a certificate' }] })

    const wrapper = await mountPage(Page)
    await flushPromises()

    expect(api.fetchKbArticles).toHaveBeenCalledWith(expect.objectContaining({ search: 'ssl' }))
    expect(wrapper.find('[data-testid="kb-search-results"]').text()).toContain('Install a certificate')
  })

  it('shows placeholders while the landing data loads', async () => {
    answerWith()
    vi.stubGlobal('useAsyncData', async () => ({ pending: ref(true) }))

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-categories"]').findAll('[data-testid="ui-skeleton"]')).toHaveLength(4)
    expect(wrapper.find('[data-testid="kb-articles"]').findAll('[data-testid="ui-skeleton"]')).toHaveLength(5)
  })
  it('shows the read failure with a retry instead of the empty text, and reads again on retry', async () => {
    api.fetchKbCategories.mockRejectedValueOnce({ data: { detail: 'Content is unavailable.' } })
    api.fetchKbArticles.mockResolvedValueOnce(ARTICLES)

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-load-error"]').text()).toContain('Content is unavailable.')
    expect(wrapper.text()).not.toContain('kb.empty')

    answerWith()
    await wrapper.find('[data-testid="kb-retry"]').trigger('click')
    await flushPromises()

    expect(api.fetchKbCategories).toHaveBeenCalledTimes(2)
    expect(wrapper.find('[data-testid="kb-load-error"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="kb-categories"]').exists()).toBe(true)
  })
})
