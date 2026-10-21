/**
 * Unit tests for NovaSecurity: a card per switched-on claim, followed by the statements that are
 * true of every install.
 *
 * @module tests/templates/nova/sections/NovaSecurity.test
 */
import { computed } from 'vue'
import { describe, expect, it, vi } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaSecurity from '~/templates/nova/sections/NovaSecurity.vue'
import { SECURITY_FEATURES } from '~/templates/nova/content'
import { SecurityFeatureKey } from '~/constants/securityFeatureKey'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

/**
 * Stands in for `useSecurityFeatures` with the given switched-on claims.
 *
 * @param keys The claims that are on.
 */
const stubFeatures = (keys: SecurityFeatureKey[]): void => {
  vi.stubGlobal('useSecurityFeatures', () => computed(() => keys))
}

describe('NovaSecurity', () => {
  it('renders a card per switched-on claim plus the always-true statements', () => {
    stubTemplateGlobals()
    stubFeatures([SecurityFeatureKey.Ssl, SecurityFeatureKey.Ddos, SecurityFeatureKey.Firewall])

    expect(mountWithKit(NovaSecurity).findAll('ul > li')).toHaveLength(3 + SECURITY_FEATURES.length)
  })

  it('states the account and isolation lines even when no claim is on', () => {
    stubTemplateGlobals()
    stubFeatures([])
    const text = mountWithKit(NovaSecurity).text()

    expect(text).toContain('nova.security.accountTitle')
    expect(text).toContain('nova.security.isolationTitle')
    expect(text).not.toContain('nova.security.sslTitle')
  })

  it('uses the DDoS wording for the DDoS claim', () => {
    stubTemplateGlobals()
    stubFeatures([SecurityFeatureKey.Ddos])

    expect(mountWithKit(NovaSecurity).text()).toContain('nova.security.ddosTitle')
  })
})
