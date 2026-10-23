/**
 * Unit tests for SectionsLocations: one card per location with a country code badge, optional
 * city and latency lines, and no section at all when there are no locations.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { reactive } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { CLASSIC_COMPONENTS } from '~/tests/helpers/classicKit'
import SectionsLocations from '~/components/sections/SectionsLocations.vue'
import type { ServerLocation } from '~/types/serverlocation'

/** The fake storefront store the section reads. */
const store = reactive({ locations: [] as ServerLocation[], loadLocations: vi.fn() })

vi.mock('~/stores/storefront', () => ({
  /**
   * Stands in for the storefront store so the section runs without transport.
   *
   * @returns The fake store.
   */
  useStorefrontStore: () => store,
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
  name: `Example DC ${id}`,
  city: 'Example City',
  latencyNote: '20 ms',
  ...overrides,
})

/**
 * Mounts the section and lets the locations load.
 *
 * @returns The wrapper.
 */
const mountSection = async () => {
  const wrapper = mountWithKit(SectionsLocations, { components: CLASSIC_COMPONENTS })
  await flushPromises()
  return wrapper
}

describe('SectionsLocations', () => {
  beforeEach(() => {
    store.loadLocations.mockReset()
    store.locations = [locationFixture(1), locationFixture(2, { countryCode: 'US', city: null, latencyNote: null })]
  })

  it('asks the store for the locations', async () => {
    await mountSection()

    expect(store.loadLocations).toHaveBeenCalled()
  })

  it('shows the heading labels', async () => {
    const wrapper = await mountSection()

    expect(wrapper.text()).toContain('hostingSections.locations.title')
  })

  it('shows the country as a code badge, never an emoji flag', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="location-code-1"]').text()).toBe('DE')
    expect(wrapper.find('[data-testid="location-code-2"]').text()).toBe('US')
  })

  it('shows city and latency only when present', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="location-city-1"]').text()).toBe('Example City')
    expect(wrapper.find('[data-testid="location-latency-1"]').text()).toBe('20 ms')
    expect(wrapper.find('[data-testid="location-city-2"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="location-latency-2"]').exists()).toBe(false)
  })

  it('lays the cards out in the balanced grid', async () => {
    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="sections-locations-grid"]').classes()).toContain('ui-balanced-grid')
  })

  it('renders nothing when there are no locations', async () => {
    store.locations = []

    const wrapper = await mountSection()

    expect(wrapper.find('[data-testid="sections-locations"]').exists()).toBe(false)
  })

  it('never caps the grid width: the shared container sets it for any number of locations', async () => {
    for (const count of [1, 2, 3]) {
      store.locations = Array.from({ length: count }, (_, index) => locationFixture(index + 1))
      const wrapper = await mountSection()
      const classes = wrapper.find('[data-testid="sections-locations-grid"]').classes()

      expect(classes.some(name => name.startsWith('max-w-'))).toBe(false)
    }
  })
})
