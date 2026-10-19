/**
 * Unit tests for TrustBar: the three guarantees.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import TrustBar from '~/templates/aurora/sections/AuroraTrustBar.vue'
import { stubAuroraGlobals } from '../auroraGlobals'

describe('TrustBar', () => {
  it('renders the three guarantees', () => {
    stubAuroraGlobals()
    const text = mountWithKit(TrustBar).text()

    expect(text).toContain('aurora.trust.ssl')
    expect(text).toContain('aurora.trust.refund')
    expect(text).toContain('aurora.trust.noHidden')
  })

  it('names no payment brand', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(TrustBar)

    expect(wrapper.find('[data-testid="trust-payment"]').exists()).toBe(false)
  })
})
