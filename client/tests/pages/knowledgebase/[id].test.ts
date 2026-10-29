/**
 * Tests for the knowledge-base article page: the body is rebuilt from an allow-list of tags
 * (never injected), the helpful vote is local, and the failure and loading states.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
import { SeoPageType } from '~/types/seopagetype'
import type { PublicPageSpies } from '~/tests/helpers/publicPage'

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

const { default: Page } = await import('~/pages/knowledgebase/[id].vue')

/** An article whose body carries markup that must not survive. */
const ARTICLE = {
  id: 5,
  title: 'Point a domain at your server',
  excerpt: 'Change the DNS records',
  views: 12,
  dateupdated: '2026-02-01',
  body: '<h2>Steps</h2><p>Open the <a href="javascript:alert(1)">panel</a>.</p><script>window.hacked = true</script>',
}

/** Spies of the stood-in runtime, renewed before each test. */
let spies: PublicPageSpies

describe('pages/knowledgebase/[id]', () => {
  beforeEach(() => {
    spies = stubPublicPageRuntime({ params: { id: '5' } })
    api.fetchKbArticle.mockReset()
  })

  it('shows the title, the update date and the view count', async () => {
    api.fetchKbArticle.mockResolvedValue(ARTICLE)

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-article-heading"]').text()).toBe('Point a domain at your server')
    expect(wrapper.find('[data-testid="kb-article-updated"]').text()).toContain('2026-02-01')
    expect(wrapper.find('[data-testid="kb-article-views"]').text()).toContain('12 kb.views')
  })

  it('renders the body through the allow-list without scripts or unsafe links', async () => {
    api.fetchKbArticle.mockResolvedValue(ARTICLE)

    const wrapper = await mountPage(Page)

    const body = wrapper.find('[data-testid="ui-rich-text"]')
    expect(body.find('h2').text()).toBe('Steps')
    expect(body.text()).toContain('Open the panel.')
    expect(body.find('script').exists()).toBe(false)
    expect(body.html()).not.toContain('javascript:')
  })

  it('links back to the knowledge base', async () => {
    api.fetchKbArticle.mockResolvedValue(ARTICLE)

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-back"]').attributes('href')).toBe('/en/knowledgebase')
  })

  it('gives the head a getter that follows the article, with a plain separator in the title', async () => {
    api.fetchKbArticle.mockResolvedValue(ARTICLE)

    await mountPage(Page)

    const options = (spies.useSeo.mock.calls[0]![0] as () => Record<string, unknown>)()
    expect(options).toEqual({
      title: 'Point a domain at your server | kb.title',
      description: 'Change the DNS records',
      type: SeoPageType.Article,
    })
    expect(String(options.title)).not.toMatch(/[–—]/)
    expect(spies.useSeoMeta).not.toHaveBeenCalled()
  })

  it('publishes Article structured data for the article at its canonical URL', async () => {
    api.fetchKbArticle.mockResolvedValue(ARTICLE)

    await mountPage(Page)

    const schema = (spies.injectSchema.mock.calls[0]![0] as () => object[])()
    expect(schema).toEqual([
      { '@type': 'Article', headline: 'Point a domain at your server', description: 'Change the DNS records', url: 'https://portal.test/canonical' },
    ])
  })

  it('highlights the chosen vote only', async () => {
    api.fetchKbArticle.mockResolvedValue(ARTICLE)
    const wrapper = await mountPage(Page)

    await wrapper.find('[data-testid="kb-vote-yes"]').trigger('click')
    expect(wrapper.find('[data-testid="kb-vote-yes"]').classes()).toContain('bg-primary-700')
    expect(wrapper.find('[data-testid="kb-vote-no"]').classes()).not.toContain('bg-red-600')

    await wrapper.find('[data-testid="kb-vote-no"]').trigger('click')
    expect(wrapper.find('[data-testid="kb-vote-no"]').classes()).toContain('bg-red-600')
    expect(wrapper.find('[data-testid="kb-vote-yes"]').classes()).not.toContain('bg-primary-700')
  })

  it.each([
    ['a 404 from the backend', { statusCode: 404, data: { detail: 'Not found.' } }, 404],
    ['a 503 from the backend', { statusCode: 503, data: { detail: 'Unavailable.' } }, 503],
    ['a network error', new TypeError('fetch failed'), 502],
  ])('raises the right status for %s', async (_name, failure, status) => {
    api.fetchKbArticle.mockRejectedValue(failure)
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    const consoleWarn = vi.spyOn(console, 'warn').mockImplementation(() => undefined)
    const errorHandler = vi.fn()

    const wrapper = await mountPage(Page, { errorHandler })

    expect(spies.createError).toHaveBeenCalledWith(expect.objectContaining({ statusCode: status }))
    expect(errorHandler).toHaveBeenCalledWith(expect.objectContaining({ statusCode: status }), expect.anything(), expect.anything())
    expect(wrapper.find('[data-testid="kb-article-heading"]').exists()).toBe(false)
    consoleError.mockRestore()
    consoleWarn.mockRestore()
  })

  it('shows placeholders while the article loads', async () => {
    api.fetchKbArticle.mockResolvedValue(ARTICLE)
    vi.stubGlobal('useAsyncData', async () => ({ pending: ref(true) }))

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="kb-article-loading"]').exists()).toBe(true)
  })
})
