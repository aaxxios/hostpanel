/**
 * Unit tests for ServiceCard: where the card links, the setup badge and the disk-usage bar.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceCard from '~/components/client/services/ServiceCard.vue'
import type { ClientService } from '~/types/clientservice'

/**
 * Builds a service with the fields the card reads.
 *
 * @param overrides - Fields to set.
 * @returns A service.
 */
const service = (overrides: Partial<ClientService> = {}): ClientService => ({
  id: 4, name: 'Starter', domain: 'example.com', groupname: 'Hosting', username: 'webmstr', status: 'Active',
  nextduedate: '2026-12-01', recurringamount: '10.00', diskusage: '256', disklimit: '1024',
  ...overrides
} as ClientService)

describe('ServiceCard', () => {
  it('links a set-up service to its page', () => {
    const wrapper = mountClient(ServiceCard, { props: { service: service(), currency: { code: 'USD' } } })

    expect(wrapper.attributes('href')).toBe('/client/services/4')
    expect(wrapper.text()).toContain('Starter')
    expect(wrapper.text()).toContain('example.com')
  })

  it('links a service without a control-panel account to its setup wizard and flags it', () => {
    const wrapper = mountClient(ServiceCard, { props: { service: service({ username: '' }), currency: {} } })

    expect(wrapper.attributes('href')).toBe('/client/services/4/setup')
    expect(wrapper.find('[data-testid="service-needs-setup"]').exists()).toBe(true)
  })

  it('shows the status badge for a set-up service', () => {
    const wrapper = mountClient(ServiceCard, { props: { service: service(), currency: {} } })

    expect(wrapper.find('[data-testid="service-needs-setup"]').exists()).toBe(false)
    expect(wrapper.text()).toContain('Active')
  })

  it('draws the disk bar only when there is a limit', () => {
    const limited = mountClient(ServiceCard, { props: { service: service(), currency: {} } })
    const unlimited = mountClient(ServiceCard, { props: { service: service({ disklimit: '0' }), currency: {} } })

    expect(limited.find('[data-testid="usage-bar"]').exists()).toBe(true)
    expect(unlimited.find('[data-testid="usage-bar"]').exists()).toBe(false)
  })

  it('formats the amount in the account currency', () => {
    const wrapper = mountClient(ServiceCard, { props: { service: service(), currency: { code: 'USD' } } })

    expect(wrapper.text()).toContain('$10.00')
  })
})
