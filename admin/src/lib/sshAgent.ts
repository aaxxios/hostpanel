/**
 * Helpers of the SSH key agent setup an administrator carries out on a managed server: the
 * one-command installer and the network lines that let only HostPanel reach the agent port.
 *
 * The installer downloads the bundle from this HostPanel, refuses to continue unless its SHA256 is
 * exactly the value the authenticated admin panel showed, unpacks it to a fresh temporary
 * directory and runs the installer with the server's profile, binding and public key. Nothing in
 * it is secret: the private key never leaves HostPanel. The network lines are a firewall rule per
 * common firewall, limited to HostPanel's addresses and the agent port, plus the sshd setting that
 * keeps OpenSSH 9.8+ from locking HostPanel out after a failed connection. Nothing here is applied
 * automatically.
 */
import type { ServerSshSetupDto, SshAgentNetworkLines } from '@/types/serverssh'

/** A bundle hash the installer can verify: 64 lowercase hex digits. */
const SHA256_HEX = /^[0-9a-f]{64}$/

/** A plain IPv4 address. */
const IPV4 = /^(25[0-5]|2[0-4]\d|1?\d?\d)(\.(25[0-5]|2[0-4]\d|1?\d?\d)){3}$/

/** A plain IPv6 address: hex groups and colons only. */
const IPV6 = /^[0-9a-fA-F:]+$/

/** Lowest valid TCP port. */
const MIN_PORT = 1

/** Highest valid TCP port. */
const MAX_PORT = 65535

/**
 * Quotes a value for a POSIX shell: single quotes, with any embedded quote closed and escaped.
 *
 * @param value - The value.
 * @returns The quoted value.
 */
export const shellQuote = (value: string): string => `'${value.replace(/'/g, `'\\''`)}'`

/**
 * The installer command for one server, or null while it cannot be built (unsupported module, no
 * identity generated yet, or a hash that is not a SHA256).
 *
 * @param origin - Origin of this HostPanel (`https://panel.example.com`).
 * @param setup - The server's setup.
 * @returns The command.
 */
export const buildInstallCommand = (origin: string, setup: ServerSshSetupDto): string | null => {
  if (!setup.profile || !setup.publicKey || !setup.binding || !SHA256_HEX.test(setup.bundleSha256)) {
    return null
  }
  const base = origin.replace(/\/+$/, '')
  // Over https, refuse any redirect to another protocol; a plain-http HostPanel (a lab) keeps the
  // hash check, which is what actually vouches for the bytes.
  const proto = base.startsWith('https://') ? "--proto '=https' " : ''
  // A subshell, so `set -e` and the trap end with the command instead of staying in the
  // administrator's interactive root shell; the trap removes the download whatever happens.
  const steps = [
    'set -e',
    'D="$(mktemp -d)"',
    `trap 'rm -rf "$D"' EXIT`,
    `curl -fsSL ${proto}-o "$D/agent.tgz" ${shellQuote(base + setup.bundlePath)}`,
    `echo "${setup.bundleSha256}  $D/agent.tgz" | sha256sum -c -`,
    'tar -xzf "$D/agent.tgz" -C "$D" --strip-components=1',
    `"$D/install.sh" --provider ${shellQuote(setup.profile)} --binding ${shellQuote(setup.binding)} ${shellQuote(setup.publicKey)}`,
  ]
  return `(${steps.join('; ')})`
}

/**
 * Whether a value is a bare IP address, so it can go into a shell line unquoted.
 *
 * @param value - The value.
 * @returns True for an IPv4 or IPv6 address.
 */
const isAddress = (value: string): boolean => IPV4.test(value) || (value.includes(':') && IPV6.test(value))

/**
 * The firewalld address family of an address.
 *
 * @param address - A bare IP address.
 * @returns `ipv6` for an IPv6 address, `ipv4` otherwise.
 */
const familyOf = (address: string): string => (address.includes(':') ? 'ipv6' : 'ipv4')

/**
 * The lines for HostPanel's addresses and the agent port, or null when there is no usable
 * address or port (the panel then asks for the address instead of showing a wrong rule).
 *
 * @param addresses - The addresses HostPanel connects from.
 * @param port - The agent's SSH port.
 * @returns The lines.
 */
export const buildNetworkLines = (addresses: string[], port: number): SshAgentNetworkLines | null => {
  if (addresses.length === 0 || !addresses.every(isAddress) || !Number.isInteger(port) || port < MIN_PORT || port > MAX_PORT) {
    return null
  }

  return {
    csf: addresses.map((a) => `csf -a ${a} 'HostPanel SSH key agent'`).join('\n'),
    firewalld: [
      ...addresses.map((a) =>
        `firewall-cmd --permanent --add-rich-rule='rule family="${familyOf(a)}" source address="${a}" port port="${port}" protocol="tcp" accept'`),
      'firewall-cmd --reload',
    ].join(' && '),
    ufw: addresses.map((a) => `ufw allow from ${a} to any port ${port} proto tcp`).join('\n'),
    sshd: `PerSourcePenaltyExemptList ${addresses.join(',')}`,
  }
}
