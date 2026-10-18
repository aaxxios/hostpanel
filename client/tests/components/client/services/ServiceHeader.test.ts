/**
 * Unit tests for ServiceHeader: the title block, the control-panel button's conditions and its event.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceHeader from '~/components/client/services/ServiceHeader.vue'
import type { ServiceDetail } from '~/types/servicedetail'
import { ServiceStatus } from '~/types/servicestatus'

/**
 * Builds a service with the fields the header reads.
 *
 * @param overrides - Fields to set.
 * @returns A service.
 */
const service = (overrides: Partial<ServiceDetail> = {}): ServiceDetail => ({
  id: 4, name: 'Starter', domain: 'example.com', groupname: 'Hosting', serverhostname: 'srv1.example.net', status: ServiceStatus.Active,
  ...overrides
} as ServiceDetail)

describe('ServiceHeader', () => {
  it('shows the plan name and the domain', () => {
    const wrapper = mountClient(ServiceHeader, { props: { service: service(), ssoLoading: false } })

    expect(wrapper.find('h1').text()).toBe('Starter')
    expect(wrapper.text()).toContain('example.com')
  })

  it('falls back to the product group when there is no domain', () => {
    const wrapper = mountClient(ServiceHeader, { props: { service: service({ domain: '' }), ssoLoading: false } })

    expect(wrapper.text()).toContain('Hosting')
  })

  it('offers the control-panel button for an active service on a known server', () => {
    const wrapper = mountClient(ServiceHeader, { props: { service: service(), ssoLoading: false } })

    expect(wrapper.find('[data-testid="service-cpanel-login"]').exists()).toBe(true)
  })

  it.each([
    [{ status: ServiceStatus.Suspended }],
    [{ serverhostname: '' }],
  ])('hides the control-panel button for %j', (overrides) => {
    const wrapper = mountClient(ServiceHeader, { props: { service: service(overrides), ssoLoading: false } })

    expect(wrapper.find('[data-testid="service-cpanel-login"]').exists()).toBe(false)
  })

  it('asks for the control panel when the button is clicked', async () => {
    const wrapper = mountClient(ServiceHeader, { props: { service: service(), ssoLoading: false } })

    await wrapper.find('[data-testid="service-cpanel-login"]').trigger('click')

    expect(wrapper.emitted('cpanel')).toHaveLength(1)
  })
})
