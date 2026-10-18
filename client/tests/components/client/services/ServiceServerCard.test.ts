/**
 * Unit tests for ServiceServerCard: the server details, the usage bars, the certificate and the links.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceServerCard from '~/components/client/services/ServiceServerCard.vue'
import type { HostingInfo } from '~/types/hostinginfo'
import type { ServiceDetail } from '~/types/servicedetail'

/**
 * Builds a service with the fields the card reads.
 *
 * @param overrides - Fields to set.
 * @returns A service.
 */
const service = (overrides: Partial<ServiceDetail> = {}): ServiceDetail => ({
  id: 4, domain: 'example.com', servername: 'srv1', serverip: '10.0.0.1', dedicatedip: '', username: 'webmstr',
  diskusage: '100', disklimit: '1000', bwusage: '5', bwlimit: '0',
  ...overrides
} as ServiceDetail)

/** A certificate that is valid. */
const validSsl: HostingInfo['ssl'] = { valid: true, issuer: 'Some CA', startDate: '2026-01-01', expiryDate: '2027-01-01' }

describe('ServiceServerCard', () => {
  it('shows the server, the IP and the nameservers', () => {
    const hostingInfo: HostingInfo = { nameservers: ['ns1.example.net', 'ns2.example.net'], ssl: null }
    const wrapper = mountClient(ServiceServerCard, { props: { service: service(), hostingInfo } })

    expect(wrapper.text()).toContain('srv1')
    expect(wrapper.text()).toContain('10.0.0.1')
    expect(wrapper.text()).toContain('ns2.example.net')
  })

  it('draws the disk bar and skips the bandwidth bar when it has no limit', () => {
    const wrapper = mountClient(ServiceServerCard, { props: { service: service(), hostingInfo: { nameservers: [], ssl: null } } })

    expect(wrapper.findAll('[data-testid="usage-bar"]')).toHaveLength(1)
  })

  it('draws both bars when both have a limit', () => {
    const wrapper = mountClient(ServiceServerCard, { props: { service: service({ bwlimit: '200' }), hostingInfo: { nameservers: [], ssl: null } } })

    expect(wrapper.findAll('[data-testid="usage-bar"]')).toHaveLength(2)
  })

  it('shows the certificate when there is one', () => {
    const wrapper = mountClient(ServiceServerCard, { props: { service: service(), hostingInfo: { nameservers: [], ssl: validSsl } } })

    expect(wrapper.find('[data-testid="service-ssl"]').text()).toContain('Some CA')
    expect(wrapper.text()).toContain('client.services.sslValid')
  })

  it('says no certificate was found for a domain without one', () => {
    const wrapper = mountClient(ServiceServerCard, { props: { service: service(), hostingInfo: { nameservers: [], ssl: null } } })

    expect(wrapper.find('[data-testid="service-ssl-missing"]').exists()).toBe(true)
  })

  it('links to the website and to the domain page only for a service with a domain', () => {
    const withDomain = mountClient(ServiceServerCard, { props: { service: service(), hostingInfo: { nameservers: [], ssl: null } } })
    const without = mountClient(ServiceServerCard, { props: { service: service({ domain: '' }), hostingInfo: { nameservers: [], ssl: null } } })

    expect(withDomain.find('[data-testid="service-visit-website"]').attributes('href')).toBe('https://example.com')
    expect(without.find('[data-testid="service-visit-website"]').exists()).toBe(false)
  })
})
