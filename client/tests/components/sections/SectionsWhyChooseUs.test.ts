/**
 * Unit tests for WhyChooseUs: the four advantages, and a title whose highlight is a slot of a
 * translation component rather than injected HTML.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import WhyChooseUs from '~/components/sections/SectionsWhyChooseUs.vue'
import MarketingSectionHeader from '~/components/marketing/MarketingSectionHeader.vue'

/** Mounts the section with the translation component stood in for. */
const mountSection = () => mountWithKit(WhyChooseUs, { components: { MarketingSectionHeader }, stubs: { 'i18n-t': true } })

describe('WhyChooseUs', () => {
  it('renders the four advantages with their copy', () => {
    const wrapper = mountSection()

    for (const key of ['fastDelivery', 'qualityGuaranteed', 'support', 'provenResults']) {
      const card = wrapper.find(`[data-testid="advantage-${key}"]`)
      expect(card.text()).toContain(`whyChooseUs.advantages.${key}.title`)
      expect(card.text()).toContain(`whyChooseUs.advantages.${key}.description`)
    }
  })

  it('builds the title with a translation component, not with markup in the message', () => {
    const wrapper = mountSection()

    expect(wrapper.html()).toContain('keypath="whyChooseUs.title"')
    expect(wrapper.find('[data-testid="section-header-title"] script').exists()).toBe(false)
  })

  it('shows the badge and the subtitle', () => {
    const wrapper = mountSection()

    expect(wrapper.text()).toContain('whyChooseUs.badge')
    expect(wrapper.text()).toContain('whyChooseUs.subtitle')
  })
})
