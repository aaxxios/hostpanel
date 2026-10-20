/**
 * Unit tests for AuroraPartners: the partners come from the storefront store, show a logo or the name, link only with a website, and the section hides when there are none.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { partnerFixture } from '~/tests/helpers/storefrontFixtures'
import AuroraPartners from '~/templates/aurora/sections/AuroraPartners.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

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
 * Mounts the section and lets the content load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  stubAuroraGlobals()
  const wrapper = mountWithKit(AuroraPartners)
  await flushPromises()
  return wrapper
}

describe('AuroraPartners', () => {
  beforeEach(() => {
    api.fetchPartners.mockReset()
    api.fetchPartners.mockResolvedValue([
      partnerFixture(1, { name: 'Alpha', logoUrl: 'https://cdn.example.test/a.png', url: 'https://alpha.example.test' }),
      partnerFixture(2, { name: 'Beta', logoUrl: null, url: null }),
    ])
  })

  it('renders the logo when a partner has one and the name otherwise', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="partner-logo-1"]').attributes('src')).toBe('https://cdn.example.test/a.png')
    expect(wrapper.find('[data-testid="partner-logo-2"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="partner-1"]').text()).toContain('Alpha')
    expect(wrapper.find('[data-testid="partner-2"]').text()).toContain('Beta')
  })

  it('links only the partners that have a website', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="partner-1"]').attributes('href')).toBe('https://alpha.example.test')
    expect(wrapper.find('[data-testid="partner-2"]').attributes('href')).toBeUndefined()
  })

  it('renders nothing while there are no partners', async () => {
    api.fetchPartners.mockResolvedValue([])

    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="aurora-partners"]').exists()).toBe(false)
    expect(wrapper.find('h2').exists()).toBe(false)
  })
})
