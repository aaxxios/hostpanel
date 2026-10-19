/**
 * Unit tests for SectionsPartners: the doubled marquee of partners read from the storefront store,
 * and the section hiding itself when there are none.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { partnerFixture } from '~/tests/helpers/storefrontFixtures'
import SectionsPartners from '~/components/sections/SectionsPartners.vue'

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

  it('lists every partner twice, so the marquee loops without a gap', async () => {
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
})
