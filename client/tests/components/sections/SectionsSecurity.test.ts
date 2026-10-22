/**
 * Unit tests for SectionsSecurity: one card per enabled protection and no section when none is
 * enabled.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import SectionsSecurity from '~/components/sections/SectionsSecurity.vue'
import { SecurityFeatureKey } from '~/constants/securityFeatureKey'
import { CLASSIC_COMPONENTS } from '~/tests/helpers/classicKit'

/** The protections the stubbed composable returns. */
const features = ref<SecurityFeatureKey[]>([])

vi.stubGlobal('useSecurityFeatures', () => features)

describe('SectionsSecurity', () => {
  beforeEach(() => {
    features.value = [SecurityFeatureKey.Ssl, SecurityFeatureKey.Firewall]
  })

  it('shows a card with title and description for each enabled protection', () => {
    const wrapper = mountWithKit(SectionsSecurity, { components: CLASSIC_COMPONENTS })

    const card = wrapper.find('[data-testid="security-ssl"]')
    expect(card.text()).toContain('hostingSections.security.features.ssl.title')
    expect(card.text()).toContain('hostingSections.security.features.ssl.description')
    expect(wrapper.find('[data-testid="security-firewall"]').exists()).toBe(true)
  })

  it('leaves out a protection that is not enabled', () => {
    const wrapper = mountWithKit(SectionsSecurity, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="security-ddos"]').exists()).toBe(false)
  })

  it('renders nothing when no protection is enabled', () => {
    features.value = []

    const wrapper = mountWithKit(SectionsSecurity, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="sections-security"]').exists()).toBe(false)
  })

  it('draws every icon on a flat brand tile with no accent glow', () => {
    const wrapper = mountWithKit(SectionsSecurity, { components: CLASSIC_COMPONENTS })

    const tiles = wrapper.findAll('[data-testid="classic-icon-tile"]')
    expect(tiles.length).toBeGreaterThan(0)
    expect(wrapper.html()).not.toContain('--accent')
  })

  it('has an icon for the DDoS protection', () => {
    features.value = [SecurityFeatureKey.Ddos]

    const wrapper = mountWithKit(SectionsSecurity, { components: CLASSIC_COMPONENTS })

    expect(wrapper.find('[data-testid="security-ddos"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="security-ddos"] [data-testid="classic-icon-tile"]').exists()).toBe(true)
  })
})
