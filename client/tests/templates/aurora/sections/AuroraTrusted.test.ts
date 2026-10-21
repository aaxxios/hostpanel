/**
 * Unit tests for AuroraTrusted: the partners as a logo strip, the name without a logo, a link only with a website, hidden when empty.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { partnerFixture } from '~/tests/helpers/storefrontFixtures'
import AuroraTrusted from '~/templates/aurora/sections/AuroraTrusted.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

/** The mocked storefront endpoints, re-created for every test. */
const api = { fetchPartners: vi.fn(), fetchTestimonials: vi.fn(), fetchProjects: vi.fn(), fetchFaqs: vi.fn(), fetchLocations: vi.fn() }

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
  const wrapper = mountWithKit(AuroraTrusted)
  await flushPromises()
  return wrapper
}

describe('AuroraTrusted', () => {
  beforeEach(() => {
    api.fetchPartners.mockReset()
    api.fetchPartners.mockResolvedValue([
      partnerFixture(1, { name: 'Alpha', logoUrl: 'https://cdn.example.test/a.png', url: 'https://alpha.example.test' }),
      partnerFixture(2, { name: 'Beta', logoUrl: null, url: null }),
    ])
  })

  it('shows the logo when a partner has one and the name otherwise', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="trusted-logo-1"]').attributes('src')).toBe('https://cdn.example.test/a.png')
    expect(wrapper.find('[data-testid="trusted-logo-2"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="trusted-2"]').text()).toContain('Beta')
  })

  it('links only the partners that have a website', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="trusted-1"]').attributes('href')).toBe('https://alpha.example.test')
    expect(wrapper.find('[data-testid="trusted-2"]').attributes('href')).toBeUndefined()
  })

  it('renders nothing while there are no partners', async () => {
    api.fetchPartners.mockResolvedValue([])

    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="aurora-trusted"]').exists()).toBe(false)
  })
})
