/**
 * Unit tests for DomainHeader: the name, the expiry line and the status.
 */
import { describe, expect, it } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import DomainHeader from '~/components/client/domains/DomainHeader.vue'
import type { DomainDetail } from '~/types/domaindetail'

/** A domain with the fields the header reads. */
const domain = { id: 1, name: 'example.com', status: 'Active', expiresAt: '2027-03-05T00:00:00Z' } as DomainDetail

describe('DomainHeader', () => {
  it('shows the full name as the page title', () => {
    const wrapper = mountClient(DomainHeader, { props: { domain, fullName: 'example.com' } })

    expect(wrapper.find('h1').text()).toBe('example.com')
  })

  it('shows the expiry line and the status', () => {
    const wrapper = mountClient(DomainHeader, { props: { domain, fullName: 'example.com' } })

    expect(wrapper.text()).toContain('client.domains.expires')
    expect(wrapper.text()).toContain('2027')
    expect(wrapper.text()).toContain('Active')
  })
})
