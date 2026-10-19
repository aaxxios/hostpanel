/**
 * Mount tests of DomainRegistrationsView (domain registrations): it renders what the API sent, shows the empty state where it
 * has one, and shows the API's sentence when the load is refused.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import DomainRegistrationsView from '@/views/DomainRegistrationsView.vue'
import { mountRoutedSettled } from '../helpers/mount'
import { json, problem } from '../helpers/fetch'
import { stubRoutes } from '../helpers/routes'

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('DomainRegistrationsView', () => {
  /** One active domain as the list endpoint returns it. */
  const domain = {
    id: 4,
    clientId: 9,
    clientName: 'Grace Hopper',
    name: 'example.com',
    regPeriodYears: 1,
    registrar: null,
    recurringAmount: 12.99,
    priceCurrency: 'USD',
    nextDueDate: '2027-01-01T00:00:00Z',
    expiresAt: '2027-01-01T00:00:00Z',
    autoRenew: true,
    status: 'Active',
  }

  it('renders the registration with its price and client link', async () => {
    stubRoutes({ 'GET /api/domains': () => json({ items: [domain], totalCount: 1 }) })
    const wrapper = await mountRoutedSettled(DomainRegistrationsView, '/clients/domain-registrations')

    const row = wrapper.get('[data-testid="domain-registrations-row-4"]').text()
    expect(row).toContain('example.com')
    expect(row).toContain('$12.99')
    expect(wrapper.get('[data-testid="domain-registrations-client-4"]').attributes('href')).toBe('/clients/9')
  })

  it('falls back when the client name is unresolved and the period is unknown', async () => {
    stubRoutes({ 'GET /api/domains': () => json({ items: [{ ...domain, clientName: null, regPeriodYears: 0 }], totalCount: 1 }) })
    const wrapper = await mountRoutedSettled(DomainRegistrationsView, '/clients/domain-registrations')

    expect(wrapper.get('[data-testid="domain-registrations-client-4"]').text()).toBe('Client #9')
    expect(wrapper.get('[data-testid="domain-registrations-row-4"]').text()).toContain('N/A')
  })

  it('pluralises the registration period', async () => {
    stubRoutes({ 'GET /api/domains': () => json({ items: [{ ...domain, regPeriodYears: 2 }], totalCount: 1 }) })
    const wrapper = await mountRoutedSettled(DomainRegistrationsView, '/clients/domain-registrations')

    expect(wrapper.get('[data-testid="domain-registrations-row-4"]').text()).toContain('2 years')
  })

  it('shows the empty state and the refusal sentence', async () => {
    stubRoutes({ 'GET /api/domains': () => json({ items: [], totalCount: 0 }) })
    const empty = await mountRoutedSettled(DomainRegistrationsView, '/clients/domain-registrations')
    expect(empty.get('[data-testid="domain-registrations-empty"]').text()).toContain('No domain registrations found')

    stubRoutes({ 'GET /api/domains': () => problem('Not allowed.', 403) })
    const refused = await mountRoutedSettled(DomainRegistrationsView, '/clients/domain-registrations')
    expect(refused.get('[data-testid="domain-registrations-error"]').text()).toContain('Not allowed.')
  })
})
