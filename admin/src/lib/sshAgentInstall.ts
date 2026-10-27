/**
 * Builds the one-command SSH key agent installer an administrator runs as root on a server.
 *
 * The command downloads the bundle from this HostPanel, refuses to continue unless its SHA256 is
 * exactly the value the authenticated admin panel showed, unpacks it to a fresh temporary
 * directory and runs the installer with the server's profile, binding and public key. Nothing in
 * it is secret: the private key never leaves HostPanel.
 */
import type { ServerSshSetupDto } from '@/types/serverssh'

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
  if (!setup.profile || !setup.publicKey || !setup.binding || !/^[0-9a-f]{64}$/.test(setup.bundleSha256)) {
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
