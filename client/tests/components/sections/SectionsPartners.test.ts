/**
 * Unit tests for SectionsPartners: a short list centred, a long one as the doubled marquee, read
 * from the storefront store, and the section hiding itself when there are none.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { partnerFixture } from '~/tests/helpers/storefrontFixtures'
import SectionsPartners from '~/components/sections/SectionsPartners.vue'
import { balancedColumns } from '~/utils/balancedGrid'

/** The mocked storefront endpoints, re-created for every test. */
const api = { fetchPartners: vi.fn(), fetchTestimonials: vi.fn(), fetchProjects: vi.fn(), fetchFaqs: vi.fn() }

vi.mock('~/composables/apis/useStorefrontApi', () => ({
  /**
   * Stands in for the storefront API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useStorefrontApi: () => api,
}))

/**
 * Mounts the section and lets the partners load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  const wrapper = mountWithKit(SectionsPartners)
  await flushPromises()
  return wrapper
}

describe('SectionsPartners', () => {
  beforeEach(() => {
    api.fetchPartners.mockReset()
    api.fetchPartners.mockResolvedValue([
      partnerFixture(1),
      partnerFixture(2, { logoUrl: 'https://example.test/logo.png' }),
    ])
  })

  it('shows the heading and its lead line', async () => {
    const wrapper = await mountSection()

    expect(wrapper.text()).toContain('clients.subtitle')
    expect(wrapper.text()).toContain('clients.heading')
  })

  it('centres a short list once instead of running a half-empty marquee', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="sections-partners-static"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="sections-partners-marquee"]').exists()).toBe(false)
    expect(wrapper.findAll('[data-testid="partner-1"]')).toHaveLength(1)
  })

  it('lists every partner twice once the strip is full, so the marquee loops without a gap', async () => {
    api.fetchPartners.mockResolvedValue([1, 2, 3, 4, 5, 6].map((id) => partnerFixture(id)))
    const wrapper = await mountSection()

    const cards = wrapper.findAll('[data-testid="partner-1"]')
    expect(cards).toHaveLength(2)
    expect(cards[0]?.text()).toContain('Example Co 1')
  })

  it('draws a logo only for a partner that has one', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="partner-logo-1"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="partner-logo-2"]').attributes('src')).toBe('https://example.test/logo.png')
  })

  it('pauses the marquee on hover', async () => {
    api.fetchPartners.mockResolvedValue([1, 2, 3, 4, 5, 6].map((id) => partnerFixture(id)))
    const wrapper = await mountSection()

    expect(wrapper.find('.animate-marquee').classes()).toContain('hover:[animation-play-state:paused]')
  })

  it('renders nothing when there are no partners', async () => {
    api.fetchPartners.mockResolvedValue([])

    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="sections-partners"]').exists()).toBe(false)
  })

  it('renders nothing when the partners cannot be read', async () => {
    api.fetchPartners.mockRejectedValue({ data: { detail: 'Unavailable.' } })

    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="sections-partners"]').exists()).toBe(false)
  })

  it('lays the cards out in the shared balanced grid, so no card is left alone on a row', async () => {
    api.fetchPartners.mockResolvedValue(Array.from({ length: 3 }, (_, index) => partnerFixture(index + 1)))

    const wrapper = await mountSection()

    const grid = wrapper.find('[data-testid="sections-partners-static"]')
    expect(grid.exists()).toBe(true)
    expect(grid.attributes('data-columns')).toBe(String(balancedColumns(3)))
    expect(grid.classes()).toContain('ui-balanced-grid')
  })
})
