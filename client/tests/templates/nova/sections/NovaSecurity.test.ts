/**
 * Unit tests for NovaSecurity: a list of security claims, where a gated claim appears once its
 * operator setting is filled in.
 *
 * @module tests/templates/nova/sections/NovaSecurity.test
 */
import { describe, expect, it } from 'vitest'
import { mountWithKit } from '~/tests/helpers/mountWithKit'
import NovaSecurity from '~/templates/nova/sections/NovaSecurity.vue'
import { SECURITY_FEATURES } from '~/templates/nova/content'
import { stubTemplateGlobals } from '~/tests/helpers/templateGlobals'

describe('NovaSecurity', () => {
  it('shows a gated claim once its setting is filled in', () => {
    // Non-null: the security list carries gated claims (firewall, scanner, backups), asserted by content.test.ts.
    const gated = SECURITY_FEATURES.find(feature => feature.requiresSetting)!
    const key = gated.requiresSetting!
    stubTemplateGlobals({ settings: { [key]: 'true' } })
    // Several claims may wait on the same setting, so count everything that setting unlocks.
    const expected = SECURITY_FEATURES.filter(feature => !feature.requiresSetting || feature.requiresSetting === key).length

    expect(mountWithKit(NovaSecurity).findAll('ul > li')).toHaveLength(expected)
  })
})
