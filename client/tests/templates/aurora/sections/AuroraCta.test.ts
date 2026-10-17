/**
 * Unit tests for AuroraCta: the closing headline and its two buttons.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraCta from '~/templates/aurora/sections/AuroraCta.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

describe('AuroraCta', () => {
  it('renders the headline and lead', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraCta)

    expect(wrapper.find('h2').text()).toBe('aurora.cta.title')
    expect(wrapper.text()).toContain('aurora.cta.lead')
  })

  it('links the primary button to contact and the secondary to hosting', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(AuroraCta)

    expect(wrapper.find('[data-testid="cta-primary"]').attributes('href')).toBe('/contact')
    expect(wrapper.find('[data-testid="cta-secondary"]').attributes('href')).toBe('/hosting')
  })
})
