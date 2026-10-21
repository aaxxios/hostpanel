/**
 * Unit tests for SectionsTrusted: the compact partner logo strip, its fallback to the name for a
 * partner without a logo, and its absence when there are no partners.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { reactive } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { partnerFixture } from '~/tests/helpers/storefrontFixtures'
import SectionsTrusted from '~/components/sections/SectionsTrusted.vue'
import type { Partner } from '~/types/partner'

/** The fake storefront store the strip reads. */
const store = reactive({ partners: [] as Partner[], loadPartners: vi.fn() })

vi.mock('~/stores/storefront', () => ({
  /**
   * Stands in for the storefront store so the strip runs without transport.
   *
   * @returns The fake store.
   */
  useStorefrontStore: () => store,
}))

/**
 * Mounts the strip and lets the partners load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  const wrapper = mountWithKit(SectionsTrusted)
  await flushPromises()
  return wrapper
}

describe('SectionsTrusted', () => {
  beforeEach(() => {
    store.loadPartners.mockReset()
    store.partners = [partnerFixture(1), partnerFixture(2, { logoUrl: 'https://example.test/logo.png' })]
  })

  it('loads the partners and shows the label', async () => {
    const wrapper = await mountSection()

    expect(store.loadPartners).toHaveBeenCalled()
    expect(wrapper.find('[data-testid="sections-trusted-label"]').text()).toBe('hostingSections.trusted.label')
  })

  it('draws a logo for a partner that has one and the name for one that has not', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="trusted-logo-2"]').attributes('src')).toBe('https://example.test/logo.png')
    expect(wrapper.find('[data-testid="trusted-logo-1"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="trusted-1"]').text()).toContain('Example Co 1')
  })

  it('is a single compact strip, not the full partners section', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="sections-partners"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="sections-trusted"]').exists()).toBe(true)
  })

  it('renders nothing when there are no partners', async () => {
    store.partners = []

    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="sections-trusted"]').exists()).toBe(false)
  })
})
