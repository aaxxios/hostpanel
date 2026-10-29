/**
 * Tests for `tests/helpers/mountClient.ts`: the component names must match the ones Nuxt
 * auto-imports, or a mounted component silently renders its children as unknown elements.
 */
import { describe, expect, it } from 'vitest'
import { mountClient, nuxtComponentName } from '~/tests/helpers/mountClient'
import ServiceCard from '~/components/client/services/ServiceCard.vue'
import SetupDomainStep from '~/components/client/services/setup/SetupDomainStep.vue'
import { makeClientService } from '~/tests/helpers/clientServiceFixture'

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
  it('resolves client components by their auto-import name', () => {
    const wrapper = mountClient(ServiceCard, { props: { service: makeClientService(), currency: {} } })

    expect(wrapper.find('[data-testid="status-badge"]').exists()).toBe(true)
  })

  it('resolves client components nested in sub-folders by their auto-import name', () => {
    const wrapper = mountClient(SetupDomainStep, { props: { domain: '', domainError: '', hasPurchasedDomain: false } })

    expect(wrapper.text()).toContain('client.setup.domainTitle')
  })
})
