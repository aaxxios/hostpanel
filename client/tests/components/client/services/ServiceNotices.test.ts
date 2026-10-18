/**
 * Unit tests for ServiceNotices: the pending-cancellation banner with its type, and the setup banner.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceNotices from '~/components/client/services/ServiceNotices.vue'
import { CancellationType } from '~/types/cancellationtype'
import type { ServiceDetail } from '~/types/servicedetail'
import { ServiceStatus } from '~/types/servicestatus'

/**
 * Builds a service in a status.
 *
 * @param status - The status.
 * @returns A service.
 */
const service = (status: ServiceStatus): ServiceDetail => ({ id: 4, status } as ServiceDetail)

describe('ServiceNotices', () => {
  it('shows nothing for an ordinary active service', () => {
    const wrapper = mountClient(ServiceNotices, { props: { service: service(ServiceStatus.Active), serviceId: '4', cancelStatus: { pending: false } } })

    expect(wrapper.find('[data-testid="service-cancel-pending"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="service-setup-required"]').exists()).toBe(false)
  })

  it.each([
    [CancellationType.Immediate, 'client.services.cancelImmediate'],
    [CancellationType.EndOfBillingPeriod, 'client.services.cancelEndOfPeriod'],
  ])('names the %s cancellation', (type, key) => {
    const wrapper = mountClient(ServiceNotices, { props: { service: service(ServiceStatus.Active), serviceId: '4', cancelStatus: { pending: true, type } } })

    expect(wrapper.find('[data-testid="service-cancel-pending"]').text()).toContain(key)
  })

  it('shows the pending notice without a type when the API sent an unknown one', () => {
    const cancelStatus = { pending: true, type: 'Something' as CancellationType }
    const wrapper = mountClient(ServiceNotices, { props: { service: service(ServiceStatus.Active), serviceId: '4', cancelStatus } })

    const banner = wrapper.find('[data-testid="service-cancel-pending"]')
    expect(banner.exists()).toBe(true)
    expect(banner.text()).not.toContain('client.services.cancelImmediate')
    expect(banner.text()).not.toContain('client.services.cancelEndOfPeriod')
  })

  it('sends a pending service to its setup wizard', () => {
    const wrapper = mountClient(ServiceNotices, { props: { service: service(ServiceStatus.Pending), serviceId: '4', cancelStatus: { pending: false } } })

    expect(wrapper.find('[data-testid="service-setup-required"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="service-complete-setup"]').attributes('href')).toBe('/client/services/4/setup')
  })
})
