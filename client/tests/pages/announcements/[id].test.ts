/**
 * Tests for the announcement detail page: the body is rebuilt from an allow-list of tags (never
 * injected), and the not-found and loading states.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountPage, stubPublicPageRuntime } from '~/tests/helpers/publicPage'
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

const { default: Page } = await import('~/pages/announcements/[id].vue')

/** An announcement whose body carries markup that must not survive. */
const ANNOUNCEMENT = {
  id: 11,
  title: 'Maintenance window',
  excerpt: 'Short outage',
  date: '2026-03-15T12:00:00Z',
  body: '<p>We will <strong>restart</strong> servers.</p><script>window.hacked = true</script><img src="x" onerror="alert(1)">',
}

/** Spies of the stood-in runtime, renewed before each test. */
let spies: PublicPageSpies

describe('pages/announcements/[id]', () => {
  beforeEach(() => {
    spies = stubPublicPageRuntime({ params: { id: '11' } })
    api.fetchAnnouncements.mockReset()
  })

  it('shows the title, the publication date and a link back to the list', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [ANNOUNCEMENT] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcement-heading"]').text()).toBe('Maintenance window')
    expect(wrapper.find('[data-testid="announcement-date"]').text()).toContain('Mar 15, 2026')
    expect(wrapper.find('[data-testid="announcement-back"]').attributes('href')).toBe('/en/announcements')
  })

  it('renders the body through the allow-list and drops scripts and handlers', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [ANNOUNCEMENT] })

    const wrapper = await mountPage(Page)

    const body = wrapper.find('[data-testid="ui-rich-text"]')
    expect(body.text()).toContain('We will restart servers.')
    expect(body.find('strong').text()).toBe('restart')
    expect(body.find('script').exists()).toBe(false)
    expect(body.html()).not.toContain('onerror')
  })

  it('sets the page title and description from the announcement', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [ANNOUNCEMENT] })

    await mountPage(Page)

    expect(spies.useSeoMeta).toHaveBeenCalledWith({ title: 'Maintenance window', description: 'Short outage' })
  })

  it('shows the not-found state for an id the list does not hold', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcement-error"]').text()).toContain('announcements.errorDesc')
    expect(wrapper.find('[data-testid="announcement-heading"]').exists()).toBe(false)
  })

  it('shows the not-found state when the list is refused', async () => {
    api.fetchAnnouncements.mockRejectedValue({ data: { detail: 'Content unavailable.' } })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcement-error"]').exists()).toBe(true)
  })

  it('shows placeholders while the announcement loads', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [ANNOUNCEMENT] })
    vi.stubGlobal('useAsyncData', async () => ({ pending: ref(true) }))

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcement-loading"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="announcement-heading"]').exists()).toBe(false)
  })
})
