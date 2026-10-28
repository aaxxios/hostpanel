/**
 * Unit tests for the firewall and sshd lines shown when a server is connected: one rule per
 * HostPanel address and firewall, scoped to the agent port, and nothing for an unusable address.
 */
import { describe, expect, it } from 'vitest'
import { buildNetworkLines } from '@/lib/sshAgentNetwork'

describe('buildNetworkLines', () => {
  it('allows only HostPanel on the agent port, per firewall, and exempts it from sshd penalties', () => {
    const lines = buildNetworkLines(['203.0.113.10'], 2222)

    expect(lines?.csf).toBe(`csf -a 203.0.113.10 'HostPanel SSH key agent'`)
    expect(lines?.firewalld).toBe(
      `firewall-cmd --permanent --add-rich-rule='rule family="ipv4" source address="203.0.113.10" port port="2222" protocol="tcp" accept' && firewall-cmd --reload`
    )
    expect(lines?.ufw).toBe(`ufw allow from 203.0.113.10 to any port 2222 proto tcp`)
    expect(lines?.sshd).toBe('PerSourcePenaltyExemptList 203.0.113.10')
  })

  it('writes one rule per address, with the IPv6 family where needed', () => {
    const lines = buildNetworkLines(['203.0.113.10', '2001:db8::10'], 22)

    expect(lines?.ufw.split('\n')).toEqual([
      'ufw allow from 203.0.113.10 to any port 22 proto tcp',
      'ufw allow from 2001:db8::10 to any port 22 proto tcp'
    ])
    expect(lines?.firewalld).toContain('rule family="ipv6" source address="2001:db8::10"')
    expect(lines?.sshd).toBe('PerSourcePenaltyExemptList 203.0.113.10,2001:db8::10')
  })

  it('builds nothing without an address, and never passes shell syntax through', () => {
    expect(buildNetworkLines([], 22)).toBeNull()
    expect(buildNetworkLines(['1.2.3.4; rm -rf /'], 22)).toBeNull()
    expect(buildNetworkLines(['203.0.113.10'], 0)).toBeNull()
  })
})
