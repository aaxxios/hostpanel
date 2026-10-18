/**
 * Unit tests for TrustBar: qualitative lines plus the operator's own uptime figure.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import TrustBar from '~/templates/nova/sections/NovaTrustBar.vue'
import { TRUST_ITEMS, TRUST_UPTIME_SETTING } from '~/templates/nova/content'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

describe('TrustBar', () => {
  it('prints no uptime figure until the operator publishes one', () => {
    stubTemplateGlobals()
    const wrapper = mountWithKit(TrustBar)

    expect(wrapper.text()).not.toContain('nova.trust.uptimeLabel')
  })

  it('prints the operator figure verbatim when set', () => {
    stubTemplateGlobals({ settings: { [TRUST_UPTIME_SETTING[0]]: '99.9%' } })
    const wrapper = mountWithKit(TrustBar)

    expect(wrapper.find('strong').text()).toBe('99.9%')
    expect(wrapper.text()).toContain('nova.trust.uptimeLabel')
  })

  it('hides gated lines until their setting is filled in', () => {
    stubTemplateGlobals()
    const gated = TRUST_ITEMS.filter(item => item.requiresSetting).length

    expect(mountWithKit(TrustBar).findAll('li')).toHaveLength(TRUST_ITEMS.length - gated)
  })
})
