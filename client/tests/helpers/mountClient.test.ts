/**
 * Tests for `tests/helpers/mountClient.ts`: the component names must match the ones Nuxt
 * auto-imports, or a mounted component silently renders its children as unknown elements.
 */
import { describe, expect, it } from 'vitest'
import { mountClient, nuxtComponentName } from '~/tests/helpers/mountClient'
import ServiceCard from '~/components/client/services/ServiceCard.vue'

describe('nuxtComponentName', () => {
  it.each([
    [['client'], 'StatusBadge', 'ClientStatusBadge'],
    [['client', 'services'], 'ServiceCard', 'ClientServicesServiceCard'],
    [['client', 'services', 'setup'], 'SetupWizard', 'ClientServicesSetupWizard'],
    [['client', 'invoices'], 'PayPanel', 'ClientInvoicesPayPanel'],
    [['client', 'domains'], 'DomainSwitchRow', 'ClientDomainsDomainSwitchRow'],
  ])('names %j/%s as %s', (folders, file, expected) => {
    expect(nuxtComponentName(folders, file)).toBe(expected)
  })
})

describe('mountClient', () => {
  it('resolves nested client components by their auto-import name', () => {
    const wrapper = mountClient(ServiceCard, {
      props: { service: { id: 1, name: 'Plan', username: 'u', status: 'Active', disklimit: '10', diskusage: '1' }, currency: {} },
    })

    expect(wrapper.find('[data-testid="usage-bar"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="status-badge"]').exists()).toBe(true)
  })
})
