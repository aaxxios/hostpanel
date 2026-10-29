/**
 * Tests for the announcement detail page: the body is rebuilt from an allow-list of tags (never
 * injected), and the not-found and loading states.
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
  fetchAnnouncement: vi.fn(),
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

const { default: Page } = await import('~/pages/announcements/[id].vue')

/** An announcement whose body carries markup that must not survive. */
const ANNOUNCEMENT = {
  id: 11,
  title: 'Maintenance window',
  publishedAt: '2026-03-15T12:00:00Z',
  content: '<p>We will <strong>restart</strong> servers.</p><script>window.hacked = true</script><img src="x" onerror="alert(1)">',
}

/** Spies of the stood-in runtime, renewed before each test. */
let spies: PublicPageSpies

describe('pages/announcements/[id]', () => {
  beforeEach(() => {
    spies = stubPublicPageRuntime({ params: { id: '11' } })
    api.fetchAnnouncement.mockReset()
  })

  it('shows the title, the publication date and a link back to the list', async () => {
    api.fetchAnnouncement.mockResolvedValue(ANNOUNCEMENT)

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcement-heading"]').text()).toBe('Maintenance window')
    expect(wrapper.find('[data-testid="announcement-date"]').text()).toContain('Mar 15, 2026')
    expect(wrapper.find('[data-testid="announcement-back"]').attributes('href')).toBe('/en/announcements')
  })

  it('renders the body through the allow-list and drops scripts and handlers', async () => {
    api.fetchAnnouncement.mockResolvedValue(ANNOUNCEMENT)

    const wrapper = await mountPage(Page)

    const body = wrapper.find('[data-testid="ui-rich-text"]')
    expect(body.text()).toContain('We will restart servers.')
    expect(body.find('strong').text()).toBe('restart')
    expect(body.find('script').exists()).toBe(false)
    expect(body.html()).not.toContain('onerror')
  })

  it('reads the announcement by its route id from the detail endpoint', async () => {
    api.fetchAnnouncement.mockResolvedValue(ANNOUNCEMENT)

    await mountPage(Page)

    expect(api.fetchAnnouncement).toHaveBeenCalledWith('11')
    expect(api.fetchAnnouncements).not.toHaveBeenCalled()
  })

  it('gives the head a getter with a plain separator and a text-only description', async () => {
    api.fetchAnnouncement.mockResolvedValue(ANNOUNCEMENT)

    await mountPage(Page)

    const options = (spies.useSeo.mock.calls[0]![0] as () => Record<string, unknown>)()
    expect(options).toEqual({
      title: 'Maintenance window | announcements.title',
      description: 'We will restart servers.',
      type: SeoPageType.Article,
    })
    expect(String(options.title)).not.toMatch(/[–—]/)
    expect(spies.useSeoMeta).not.toHaveBeenCalled()
  })

  it('publishes Article structured data with the publication date', async () => {
    api.fetchAnnouncement.mockResolvedValue(ANNOUNCEMENT)

    await mountPage(Page)

    const schema = (spies.injectSchema.mock.calls[0]![0] as () => object[])()
    expect(schema).toEqual([
      {
        '@type': 'Article',
        headline: 'Maintenance window',
        description: 'We will restart servers.',
        url: 'https://portal.test/canonical',
        datePublished: '2026-03-15T12:00:00Z',
      },
    ])
  })

  it.each([
    ['an unknown or unpublished id', () => api.fetchAnnouncement.mockRejectedValue({ statusCode: 404, data: { detail: 'Not found.' } }), 404],
    ['an empty answer', () => api.fetchAnnouncement.mockResolvedValue(null), 404],
    ['a 503 from the backend', () => api.fetchAnnouncement.mockRejectedValue({ statusCode: 503, data: { detail: 'Unavailable.' } }), 503],
    ['a network error', () => api.fetchAnnouncement.mockRejectedValue(new TypeError('fetch failed')), 502],
  ])('raises the right status for %s', async (_name, arrange, status) => {
    arrange()
    const consoleError = vi.spyOn(console, 'error').mockImplementation(() => undefined)
    const consoleWarn = vi.spyOn(console, 'warn').mockImplementation(() => undefined)
    const errorHandler = vi.fn()

    const wrapper = await mountPage(Page, { errorHandler })

    expect(spies.createError).toHaveBeenCalledWith(expect.objectContaining({ statusCode: status }))
    expect(errorHandler).toHaveBeenCalledWith(expect.objectContaining({ statusCode: status }), expect.anything(), expect.anything())
    expect(wrapper.find('[data-testid="announcement-heading"]').exists()).toBe(false)
    consoleError.mockRestore()
    consoleWarn.mockRestore()
  })

  it('shows placeholders while the announcement loads', async () => {
    api.fetchAnnouncement.mockResolvedValue(ANNOUNCEMENT)
    vi.stubGlobal('useAsyncData', async () => ({ pending: ref(true) }))

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcement-loading"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="announcement-heading"]').exists()).toBe(false)
  })
})
