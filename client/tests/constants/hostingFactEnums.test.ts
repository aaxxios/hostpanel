/**
 * Unit tests for the hosting fact enums: the wire values the settings, the locale keys and the
 * sections rely on.
 *
 * @module tests/constants/hostingFactEnums.test
 */

import { describe, expect, it } from 'vitest'
import { GuaranteeKey } from '~/constants/guaranteeKey'
import { InstallerAppCategory } from '~/constants/installerAppCategory'
import { PerformanceStatKey } from '~/constants/performanceStatKey'
import { SecurityFeatureKey } from '~/constants/securityFeatureKey'
import { SupportChannel } from '~/constants/supportChannel'

describe('hosting fact enums', () => {
  it('lists the performance figures in display order', () => {
    expect(Object.values(PerformanceStatKey)).toEqual(['uptime', 'response_time', 'sites_hosted', 'support_response'])
  })

  it('lists the security protections in display order', () => {
    expect(Object.values(SecurityFeatureKey)).toEqual(['ssl', 'backups', 'ddos', 'malware', 'firewall'])
  })

  it('lists the guarantees in display order', () => {
    expect(Object.values(GuaranteeKey)).toEqual(['free_migration', 'money_back', 'no_hidden_fees'])
  })

  it('lists the support channels in display order', () => {
    expect(Object.values(SupportChannel)).toEqual(['chat', 'ticket', 'email', 'phone'])
  })

  it('lists the installer categories in display order', () => {
    expect(Object.values(InstallerAppCategory)).toEqual(['cms', 'shop', 'forum', 'wiki', 'blog', 'gallery'])
  })
})
