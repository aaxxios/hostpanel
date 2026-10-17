/**
 * Unit tests for ServiceDetailsCard: the description list and its fallbacks.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceDetailsCard from '~/components/client/services/ServiceDetailsCard.vue'
import type { ServiceDetail } from '~/types/servicedetail'

/**
 * Builds a service with the fields the card reads.
 *
 * @param overrides - Fields to set.
 * @returns A service.
 */
const service = (overrides: Partial<ServiceDetail> = {}): ServiceDetail => ({
  id: 4, name: 'Starter', domain: 'example.com', status: 'Active', billingcycle: 'monthly', regdate: '2026-01-01', nextduedate: '2026-12-01',
  recurringamount: '10.00', firstpaymentamount: '12.00', paymentmethod: 'stripe', paymentmethodname: 'Card',
  ...overrides
} as ServiceDetail)

describe('ServiceDetailsCard', () => {
  it('lists the plan, the domain and the billing cycle', () => {
    const wrapper = mountClient(ServiceDetailsCard, { props: { service: service(), currency: { code: 'USD' } } })

    expect(wrapper.text()).toContain('Starter')
    expect(wrapper.text()).toContain('example.com')
    expect(wrapper.text()).toContain('monthly')
  })

  it('formats both amounts in the account currency', () => {
    const wrapper = mountClient(ServiceDetailsCard, { props: { service: service(), currency: { code: 'USD' } } })

    expect(wrapper.text()).toContain('$10.00')
    expect(wrapper.text()).toContain('$12.00')
  })

  it('prefers the payment method name over the module name', () => {
    const wrapper = mountClient(ServiceDetailsCard, { props: { service: service(), currency: {} } })

    expect(wrapper.text()).toContain('Card')
  })

  it('shows a dash for a missing domain', () => {
    const wrapper = mountClient(ServiceDetailsCard, { props: { service: service({ domain: '' }), currency: {} } })

    expect(wrapper.text()).toContain('—')
  })
})
