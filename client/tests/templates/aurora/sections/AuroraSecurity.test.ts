/**
 * Unit tests for AuroraSecurity: one card per feature the composable reports and nothing without any.
 */
import { describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import AuroraSecurity from '~/templates/aurora/sections/AuroraSecurity.vue'
import { SecurityFeatureKey } from '~/constants/securityFeatureKey'
import { stubAuroraGlobals } from '../auroraGlobals'
import { balancedColumns } from '~/utils/balancedGrid'

/** The features the mocked composable reports. */
const features = vi.hoisted(() => ({ value: [] as SecurityFeatureKey[] }))

vi.mock('~/composables/useSecurityFeatures', () => ({
  /**
   * Stands in for the features composable.
   *
   * @returns The mocked features as a ref.
   */
  useSecurityFeatures: () => ref(features.value),
}))

describe('AuroraSecurity', () => {
  it('renders a card for each feature that is on and none for the others', () => {
    features.value = [SecurityFeatureKey.Ssl, SecurityFeatureKey.Ddos]
    stubAuroraGlobals()

    const wrapper = mountWithKit(AuroraSecurity)

    expect(wrapper.find('[data-testid="security-ssl"]').text()).toContain('aurora.security.sslTitle')
    expect(wrapper.find('[data-testid="security-ddos"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="security-firewall"]').exists()).toBe(false)
  })

  it('renders nothing when no feature is on', () => {
    features.value = []
    stubAuroraGlobals()

    expect(mountWithKit(AuroraSecurity).find('[data-testid="aurora-security"]').exists()).toBe(false)
  })

  it('lays the cards out in the balanced grid', () => {
    features.value = [SecurityFeatureKey.Ssl, SecurityFeatureKey.Backups, SecurityFeatureKey.Ddos, SecurityFeatureKey.Malware, SecurityFeatureKey.Firewall]
    stubAuroraGlobals()

    const grid = mountWithKit(AuroraSecurity).find('[data-testid="aurora-security-grid"]')

    expect(grid.attributes('data-columns')).toBe(String(balancedColumns(5)))
  })
})
