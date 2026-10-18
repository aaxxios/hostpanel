/**
 * Unit tests for FinalCta: the closing heading and its two calls to action.
 */
import { beforeEach, describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import FinalCta from '~/templates/nova/sections/FinalCta.vue'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

describe('FinalCta', () => {
  beforeEach(() => {
    stubTemplateGlobals()
  })

  it('states the title and lead', () => {
    const wrapper = mountWithKit(FinalCta)

    expect(wrapper.find('h2').text()).toBe('nova.finalCta.title')
    expect(wrapper.text()).toContain('nova.finalCta.lead')
  })

  it('links the primary action to hosting and the secondary to the plans anchor', () => {
    const wrapper = mountWithKit(FinalCta)

    expect(wrapper.find('[data-testid="nova-final-cta-primary"]').attributes('href')).toBe('/hosting')
    expect(wrapper.find('[data-testid="nova-final-cta-secondary"]').attributes('href')).toBe('/hosting#plans')
  })
})
