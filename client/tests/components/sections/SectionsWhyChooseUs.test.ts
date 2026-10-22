/**
 * Unit tests for WhyChooseUs: the four advantages on flat classic cards, with a plain-text title
 * (no highlighted word) and the badge and subtitle of the section header.
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import { CLASSIC_COMPONENTS } from '~/tests/helpers/classicKit'
import WhyChooseUs from '~/components/sections/SectionsWhyChooseUs.vue'

/** Mounts the section. */
const mountSection = () => mountWithKit(WhyChooseUs, { components: CLASSIC_COMPONENTS })

describe('WhyChooseUs', () => {
  it('renders the four advantages with their copy', () => {
    const wrapper = mountSection()

    for (const key of ['fastDelivery', 'qualityGuaranteed', 'support', 'provenResults']) {
      const card = wrapper.find(`[data-testid="advantage-${key}"]`)
      expect(card.text()).toContain(`whyChooseUs.advantages.${key}.title`)
      expect(card.text()).toContain(`whyChooseUs.advantages.${key}.description`)
    }
  })

  it('builds the title from one plain translation, with no highlighted word', () => {
    const wrapper = mountSection()

    expect(wrapper.find('[data-testid="classic-section-header-title"]').text()).toBe('whyChooseUs.title')
    expect(wrapper.find('[data-testid="classic-section-header-title"] span').exists()).toBe(false)
  })

  it('shows the badge and the subtitle', () => {
    const wrapper = mountSection()

    expect(wrapper.text()).toContain('whyChooseUs.badge')
    expect(wrapper.text()).toContain('whyChooseUs.subtitle')
  })

  it('is flat: no gradient, glow blob or particle', () => {
    const html = mountSection().html()

    expect(html).not.toContain('gradient')
    expect(html).not.toContain('blur-')
    expect(html).not.toContain('animate-float')
  })
})
