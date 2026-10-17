/**
 * Unit tests for ProductsBenefits: the four platform benefits and the three reassurances.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import ProductsBenefits from '~/components/catalog/ProductsBenefits.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'

/** Mounts the block with the translation component stood in for. */
const mountBenefits = () => mountWithKit(ProductsBenefits, { components: { MarketingSectionHeader }, stubs: { 'i18n-t': true } })

describe('ProductsBenefits', () => {
  it('renders the four benefits with their titles and descriptions', () => {
    const wrapper = mountBenefits()

    for (const key of ['whiteLabel', 'support', 'updates', 'api']) {
      const card = wrapper.find(`[data-testid="benefit-${key}"]`)
      expect(card.text()).toContain(`products.benefits.${key}.title`)
      expect(card.text()).toContain(`products.benefits.${key}.description`)
    }
  })

  it('renders the three reassurances', () => {
    const wrapper = mountBenefits()

    for (const key of ['trial', 'guarantee', 'lifetime']) {
      const card = wrapper.find(`[data-testid="reassurance-${key}"]`)
      expect(card.text()).toContain(`products.benefits.additional.${key}.title`)
      expect(card.text()).toContain(`products.benefits.additional.${key}.description`)
    }
  })

  it('shows the subtitle under the header', () => {
    expect(mountBenefits().text()).toContain('products.benefits.subtitle')
  })
})
