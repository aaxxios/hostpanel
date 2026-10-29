/**
 * Unit tests for ServiceServerCard: the server details, the certificate and the links.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceServerCard from '~/components/client/services/ServiceServerCard.vue'
import type { HostingInfo } from '~/types/hostinginfo'
import { makeClientService as service } from '~/tests/helpers/clientServiceFixture'

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

  it('shows a dash for a service with no server assigned', () => {
    const wrapper = mountClient(ServiceServerCard, {
      props: { service: service({ server: null }), hostingInfo: { nameservers: [], ssl: null } }
    })

    expect(wrapper.text()).not.toContain('srv1')
    expect(wrapper.text()).toContain('—')
  })

  it('shows the certificate when there is one', () => {
    const wrapper = mountClient(ServiceServerCard, { props: { service: service(), hostingInfo: { nameservers: [], ssl: validSsl } } })

    expect(wrapper.find('[data-testid="service-ssl"]').text()).toContain('Some CA')
    // The backend sends ISO dates; the card renders them as readable calendar dates.
    expect(wrapper.find('[data-testid="service-ssl"]').text()).toContain('Jan 1, 2026')
    expect(wrapper.find('[data-testid="service-ssl"]').text()).toContain('Jan 1, 2027')
    expect(wrapper.text()).toContain('client.services.sslValid')
  })

  it('says no certificate was found for a domain without one', () => {
    const wrapper = mountClient(ServiceServerCard, { props: { service: service(), hostingInfo: { nameservers: [], ssl: null } } })

    expect(wrapper.find('[data-testid="service-ssl-missing"]').exists()).toBe(true)
  })

  it('links to the website and to the domain page only for a service with a domain', () => {
    const withDomain = mountClient(ServiceServerCard, { props: { service: service(), hostingInfo: { nameservers: [], ssl: null } } })
    const without = mountClient(ServiceServerCard, { props: { service: service({ domain: null }), hostingInfo: { nameservers: [], ssl: null } } })

    expect(withDomain.find('[data-testid="service-visit-website"]').attributes('href')).toBe('https://example.com')
    expect(without.find('[data-testid="service-visit-website"]').exists()).toBe(false)
  })

  it('draws a bar for each limited quota the server reported', () => {
    const usage = { diskUsedMb: 256, diskLimitMb: 1024, bandwidthUsedMb: 10, bandwidthLimitMb: 0, updatedAt: '2026-10-01T00:00:00Z' }
    const wrapper = mountClient(ServiceServerCard, { props: { service: service({ usage }), hostingInfo: { nameservers: [], ssl: null } } })

    expect(wrapper.find('[data-testid="service-usage"]').findAll('[data-testid="usage-bar"]')).toHaveLength(1)
    expect(wrapper.find('[data-testid="service-usage"]').text()).toContain('client.services.disk')
  })

  it('draws the bandwidth bar alone when only bandwidth is limited', () => {
    const usage = { diskUsedMb: 256, diskLimitMb: 0, bandwidthUsedMb: 10, bandwidthLimitMb: 2048, updatedAt: '2026-10-01T00:00:00Z' }
    const wrapper = mountClient(ServiceServerCard, { props: { service: service({ usage }), hostingInfo: { nameservers: [], ssl: null } } })

    const block = wrapper.find('[data-testid="service-usage"]')
    expect(block.findAll('[data-testid="usage-bar"]')).toHaveLength(1)
    expect(block.text()).toContain('client.services.bandwidth')
    expect(block.text()).not.toContain('client.services.disk')
  })

  it('draws no usage when neither quota is limited', () => {
    const usage = { diskUsedMb: 256, diskLimitMb: 0, bandwidthUsedMb: 10, bandwidthLimitMb: 0, updatedAt: '2026-10-01T00:00:00Z' }
    const wrapper = mountClient(ServiceServerCard, { props: { service: service({ usage }), hostingInfo: { nameservers: [], ssl: null } } })

    expect(wrapper.find('[data-testid="service-usage"]').exists()).toBe(false)
  })

  it('draws no usage when the server reported none', () => {
    const wrapper = mountClient(ServiceServerCard, { props: { service: service(), hostingInfo: { nameservers: [], ssl: null } } })

    expect(wrapper.find('[data-testid="service-usage"]').exists()).toBe(false)
  })
})
