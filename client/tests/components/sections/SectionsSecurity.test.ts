/**
 * Unit tests for SectionsSecurity: one card per enabled protection and no section when none is
 * enabled.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsSecurity from '~/components/sections/SectionsSecurity.vue'
import { SecurityFeatureKey } from '~/constants/securityFeatureKey'
import { ACCENT_BRAND_CLASS } from '~/utils/ui/accent'

/** The protections the stubbed composable returns. */
const features = ref<SecurityFeatureKey[]>([])

vi.stubGlobal('useSecurityFeatures', () => features)

describe('SectionsSecurity', () => {
  beforeEach(() => {
    features.value = [SecurityFeatureKey.Ssl, SecurityFeatureKey.Firewall]
  })

  it('shows a card with title and description for each enabled protection', () => {
    const wrapper = mountWithKit(SectionsSecurity)

    const card = wrapper.find('[data-testid="security-ssl"]')
    expect(card.text()).toContain('hostingSections.security.features.ssl.title')
    expect(card.text()).toContain('hostingSections.security.features.ssl.description')
    expect(wrapper.find('[data-testid="security-firewall"]').exists()).toBe(true)
  })

  it('leaves out a protection that is not enabled', () => {
    const wrapper = mountWithKit(SectionsSecurity)

    expect(wrapper.find('[data-testid="security-ddos"]').exists()).toBe(false)
  })

  it('renders nothing when no protection is enabled', () => {
    features.value = []

    const wrapper = mountWithKit(SectionsSecurity)

    expect(wrapper.find('[data-testid="sections-security"]').exists()).toBe(false)
  })

  it('points every tile at the brand colour so it is visible on a light page', () => {
    const wrapper = mountWithKit(SectionsSecurity)

    const tiles = wrapper.findAll('[class*="--accent:"]')
    expect(tiles.length).toBeGreaterThan(0)
    for (const tile of tiles) {
      expect(tile.classes()).toContain(ACCENT_BRAND_CLASS)
    }
  })

  it('has an icon for the DDoS protection', () => {
    features.value = [SecurityFeatureKey.Ddos]

    const wrapper = mountWithKit(SectionsSecurity)

    expect(wrapper.find('[data-testid="security-ddos"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="security-ddos"] [class*="--accent:"]').exists()).toBe(true)
  })
})
