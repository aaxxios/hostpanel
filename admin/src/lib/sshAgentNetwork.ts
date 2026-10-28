/**
 * Builds the lines an administrator adds on a managed server so HostPanel, and only HostPanel,
 * can reach its SSH key agent: a firewall rule for each common firewall, limited to HostPanel's
 * addresses and the agent port, and the sshd setting that keeps OpenSSH 9.8+ from locking
 * HostPanel out after a failed connection. Nothing here is applied automatically.
 */

/** The ready-made lines, one string per tool. */
export interface SshAgentNetworkLines {
  /** ConfigServer Firewall (CWP's default): allow each address. */
  csf: string
  /** firewalld: one permanent rich rule per address, then a reload. */
  firewalld: string
  /** ufw: one rule per address. */
  ufw: string
  /** sshd_config line exempting HostPanel from PerSourcePenalties. */
  sshd: string
}

/** A plain IPv4 address. */
const IPV4 = /^(25[0-5]|2[0-4]\d|1?\d?\d)(\.(25[0-5]|2[0-4]\d|1?\d?\d)){3}$/

/** A plain IPv6 address: hex groups and colons only. */
const IPV6 = /^[0-9a-fA-F:]+$/

/**
 * Whether a value is a bare IP address, so it can go into a shell line unquoted.
 *
 * @param value - The value.
 * @returns True for an IPv4 or IPv6 address.
 */
const isAddress = (value: string): boolean => IPV4.test(value) || (value.includes(':') && IPV6.test(value))

/**
 * The lines for HostPanel's addresses and the agent port, or null when there is no usable
 * address or port (the panel then asks for the address instead of showing a wrong rule).
 *
 * @param addresses - The addresses HostPanel connects from.
 * @param port - The agent's SSH port.
 * @returns The lines.
 */
export const buildNetworkLines = (addresses: string[], port: number): SshAgentNetworkLines | null => {
  if (addresses.length === 0 || !addresses.every(isAddress) || !Number.isInteger(port) || port < 1 || port > 65535) {
    return null
  }

  return {
    csf: addresses.map((a) => `csf -a ${a} 'HostPanel SSH key agent'`).join('\n'),
    firewalld: [
      ...addresses.map((a) =>
        `firewall-cmd --permanent --add-rich-rule='rule family="${a.includes(':') ? 'ipv6' : 'ipv4'}" source address="${a}" port port="${port}" protocol="tcp" accept'`),
      'firewall-cmd --reload'
    ].join(' && '),
    ufw: addresses.map((a) => `ufw allow from ${a} to any port ${port} proto tcp`).join('\n'),
    sshd: `PerSourcePenaltyExemptList ${addresses.join(',')}`
  }
}
