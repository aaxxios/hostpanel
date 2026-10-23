/**
 * Unit tests for ProductsBenefits: the four customer benefits and the three reassurances, with no
 * reseller copy left.
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

    for (const key of ['speed', 'security', 'support', 'migration']) {
      const card = wrapper.find(`[data-testid="benefit-${key}"]`)
      expect(card.text()).toContain(`products.benefits.${key}.title`)
      expect(card.text()).toContain(`products.benefits.${key}.description`)
    }
  })

  it('has no reseller benefits', () => {
    const wrapper = mountBenefits()

    for (const key of ['whiteLabel', 'api', 'updates']) {
      expect(wrapper.find(`[data-testid="benefit-${key}"]`).exists()).toBe(false)
    }
  })

  it('is never rendered hidden: no scroll-triggered initial state, a CSS entrance instead', () => {
    const wrapper = mountBenefits()
    const card = wrapper.find('[data-testid="benefit-speed"]')

    expect(card.attributes('style') ?? '').not.toContain('opacity')
    expect(card.classes()).toContain('animate-reveal-up')
  })

  it('renders the three reassurances', () => {
    const wrapper = mountBenefits()

    for (const key of ['trial', 'guarantee', 'pricing']) {
      const card = wrapper.find(`[data-testid="reassurance-${key}"]`)
      expect(card.text()).toContain(`products.benefits.additional.${key}.title`)
      expect(card.text()).toContain(`products.benefits.additional.${key}.description`)
    }
  })

  it('shows the subtitle under the header', () => {
    expect(mountBenefits().text()).toContain('products.benefits.subtitle')
  })
})
