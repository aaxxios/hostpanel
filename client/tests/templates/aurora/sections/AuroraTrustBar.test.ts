/**
 * Unit tests for TrustBar: the three guarantees and the accepted payment methods.
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

  it('lists the four payment brands untranslated', () => {
    stubAuroraGlobals()
    const wrapper = mountWithKit(TrustBar)
    const brands = wrapper.findAll('[data-testid="trust-payment"]').map((chip) => chip.text())

    expect(brands).toEqual(['VISA', 'Mastercard', 'PayPal', 'Idram'])
  })
})
