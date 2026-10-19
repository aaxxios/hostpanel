/**
 * Unit tests for AuroraServices: four service cards, each with a named gradient tile.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraServices from '~/templates/aurora/sections/AuroraServices.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

describe('AuroraServices', () => {
  it('renders the four services with their copy', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraServices)

    for (const tag of ['hosting', 'servers', 'domains', 'billing']) {
      expect(wrapper.find(`[data-testid="service-card-${tag}"]`).exists()).toBe(true)
    }
    expect(wrapper.text()).toContain('aurora.services.domainsBody')
  })

  it('paints each tile from a gradient token, never an inline style', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraServices)
    const tile = wrapper.find('[data-testid="service-card-hosting"]').find('.place-items-center')

    expect(tile.classes()).toContain('bg-tile-sky')
    expect(wrapper.html()).not.toContain(' style=')
  })
})
