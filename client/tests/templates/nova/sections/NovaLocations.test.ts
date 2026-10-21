/**
 * Unit tests for NovaLocations: the locations come from the storefront store, show code, name,
 * city and latency note, and the section hides when there are none.
 *
 * @module tests/templates/nova/sections/NovaLocations.test
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaLocations from '~/templates/nova/sections/NovaLocations.vue'
import type { ServerLocation } from '~/types/serverlocation'
import { balancedColumns } from '~/utils/balancedGrid'

/** The mocked storefront endpoints, re-created for every test. */
const api = { fetchPartners: vi.fn(), fetchLocations: vi.fn(), fetchTestimonials: vi.fn(), fetchProjects: vi.fn(), fetchFaqs: vi.fn() }

vi.mock('~/composables/apis/useStorefrontApi', () => ({
  /**
   * Stands in for the storefront API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useStorefrontApi: () => api,
}))

/**
 * Builds a location.
 *
 * @param id The location id.
 * @param overrides Fields that differ from the defaults.
 * @returns The location.
 */
const locationFixture = (id: number, overrides: Partial<ServerLocation> = {}): ServerLocation => ({
  id,
  countryCode: 'DE',
  name: `Example Region ${id}`,
  city: null,
  latencyNote: null,
  ...overrides,
})

/**
 * Mounts the section and lets the content load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  const wrapper = mountWithKit(NovaLocations)
  await flushPromises()
  return wrapper
}

describe('NovaLocations', () => {
  beforeEach(() => {
    api.fetchLocations.mockReset()
    api.fetchLocations.mockResolvedValue([
      locationFixture(1, { countryCode: 'FR', name: 'Alpha', city: 'Paris', latencyNote: 'Under 20 ms in Europe' }),
      locationFixture(2, { name: 'Beta' }),
    ])
  })

  it('shows the code, name, city and latency note of a location', async () => {
    const text = (await mountSection()).find('[data-testid="nova-location-1"]').text()

    expect(text).toContain('FR')
    expect(text).toContain('Alpha')
    expect(text).toContain('Paris')
    expect(text).toContain('Under 20 ms in Europe')
  })

  it('leaves out the city and the note when a location has none', async () => {
    const card = (await mountSection()).find('[data-testid="nova-location-2"]')

    expect(card.text()).toContain('Beta')
    expect(card.findAll('p')).toHaveLength(0)
  })

  it('lays the cards out in the balanced grid', async () => {
    const grid = (await mountSection()).find('[data-testid="nova-locations-grid"]')

    expect(grid.attributes('data-columns')).toBe(String(balancedColumns(2)))
  })

  it('renders nothing while there are no locations', async () => {
    api.fetchLocations.mockResolvedValue([])

    expect((await mountSection()).find('[data-testid="nova-locations"]').exists()).toBe(false)
  })
})
