/**
 * Tests for the announcements list page: the list read through the content store, month filters
 * built from the publication dates, and the loading, error and empty states.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
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

const { default: Page } = await import('~/pages/announcements/index.vue')

/** Two announcements in different months. */
const TWO_MONTHS = {
  items: [
    { id: 11, title: 'Maintenance window', publishedAt: '2026-03-15T12:00:00Z', content: '<p>x</p>' },
    { id: 12, title: 'New data centre', publishedAt: '2026-02-10T12:00:00Z', content: '<p>y</p>' },
  ],
}

/** Spies of the stood-in runtime, renewed before each test. */
let spies: PublicPageSpies

describe('pages/announcements/index', () => {
  beforeEach(() => {
    spies = stubPublicPageRuntime()
    vi.stubGlobal('useCookie', () => ref(null))
    api.fetchAnnouncements.mockReset()
  })

  it('shows the hero and sets the page title', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('h1').text()).toBe('announcements.title')
    expect(spies.useSeoMeta).toHaveBeenCalledWith({
      title: 'announcements.title',
      description: 'announcements.subtitle',
    })
  })

  it('lists each announcement as a link to its page with the formatted date', async () => {
    api.fetchAnnouncements.mockResolvedValue(TWO_MONTHS)

    const wrapper = await mountPage(Page)

    const first = wrapper.find('[data-testid="announcement-11"]')
    expect(first.attributes('href')).toBe('/en/announcements/11')
    expect(first.text()).toContain('Maintenance window')
    expect(first.text()).toContain('Mar 15, 2026')
    expect(wrapper.findAll('[data-testid^="announcement-"]')).toHaveLength(2)
  })

  it('offers a month filter and narrows the list to the chosen month', async () => {
    api.fetchAnnouncements.mockResolvedValue(TWO_MONTHS)
    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcements-months"]').exists()).toBe(true)
    await wrapper.find('[data-testid="announcements-month-February 2026"]').trigger('click')

    expect(wrapper.find('[data-testid="announcement-12"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="announcement-11"]').exists()).toBe(false)

    await wrapper.find('[data-testid="announcements-month-all"]').trigger('click')
    expect(wrapper.findAll('[data-testid^="announcement-"]')).toHaveLength(2)
  })

  it('offers no month filter when every announcement is from one month', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [TWO_MONTHS.items[0]] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcements-months"]').exists()).toBe(false)
  })

  it('shows the empty state when there is nothing to list', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [] })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcements-empty"]').text()).toContain('announcements.empty')
  })

  it('shows the error alert when a signed-in visitor is refused the list', async () => {
    vi.stubGlobal('useCookie', () => ref('1'))
    api.fetchAnnouncements.mockRejectedValue({ data: { detail: 'Content unavailable.' } })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcements-error"]').text()).toContain('announcements.errorDesc')
    expect(wrapper.find('[data-testid="announcements-empty"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="announcements-sign-in"]').exists()).toBe(false)
  })

  it('invites a signed-out visitor to sign in instead of showing an error', async () => {
    api.fetchAnnouncements.mockRejectedValue({ statusCode: 401, data: { detail: 'Not authorized.' } })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcements-sign-in"]').text()).toContain('announcements.signInRequired')
    expect(wrapper.find('[data-testid="announcements-sign-in-link"]').attributes('href')).toBe('/en/client/login?redirect=/announcements')
    expect(wrapper.find('[data-testid="announcements-error"]').exists()).toBe(false)
  })

  it('shows placeholders while the list loads', async () => {
    api.fetchAnnouncements.mockResolvedValue({ items: [] })
    vi.stubGlobal('useAsyncData', async () => {
      await flushPromises()
      return { pending: ref(true) }
    })

    const wrapper = await mountPage(Page)

    expect(wrapper.find('[data-testid="announcements-loading"]').findAll('[data-testid="ui-skeleton"]')).toHaveLength(4)
  })
})
