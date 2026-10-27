/**
 * Unit tests for the one-command installer builder: it verifies the hash before unpacking, quotes
 * every value, and refuses to build a command it cannot make safe.
 */
import { describe, expect, it } from 'vitest'
import { buildInstallCommand, shellQuote } from '@/lib/sshAgentInstall'
import type { ServerSshSetupDto } from '@/types/serverssh'

/** A complete setup. */
const SETUP: ServerSshSetupDto = {
  serverId: 4,
  profile: 'cwp',
  publicKey: 'ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b hostpanel-server-4',
  binding: '11111111-1111-4111-8111-111111111111',
  agentUsername: 'hpsshkeys',
  bundleVersion: '1.1.0',
  bundleSha256: 'a'.repeat(64),
  bundlePath: '/api/public/ssh-agent/bundle.tar.gz',
}

describe('buildInstallCommand', () => {
  it('downloads from this HostPanel, checks the hash, then installs with profile, binding and key', () => {
    const command = buildInstallCommand('https://panel.example.com/', SETUP) ?? ''

    const steps = command.split(' && ')
    expect(steps[2]).toBe(`curl -fsSL --proto '=https' -o "$D/agent.tgz" 'https://panel.example.com/api/public/ssh-agent/bundle.tar.gz'`)
    expect(steps[3]).toBe(`echo "${'a'.repeat(64)}  $D/agent.tgz" | sha256sum -c -`)
    expect(steps.indexOf(steps[3]!)).toBeLessThan(steps.findIndex((s) => s.startsWith('tar ')))
    expect(steps[5]).toBe(`"$D/install.sh" --provider 'cwp' --binding '${SETUP.binding}' '${SETUP.publicKey}'`)
    expect(command).not.toContain('PRIVATE')
  })

  it('keeps the hash check but drops the https-only flag for a plain-http lab', () => {
    const command = buildInstallCommand('http://localhost:5148', SETUP) ?? ''

    expect(command).toContain('sha256sum -c -')
    expect(command).not.toContain('--proto')
  })

  it('builds nothing without a profile, an identity or a real hash', () => {
    expect(buildInstallCommand('https://p', { ...SETUP, profile: null })).toBeNull()
    expect(buildInstallCommand('https://p', { ...SETUP, publicKey: null })).toBeNull()
    expect(buildInstallCommand('https://p', { ...SETUP, binding: null })).toBeNull()
    expect(buildInstallCommand('https://p', { ...SETUP, bundleSha256: 'abc; rm -rf /' })).toBeNull()
  })
})

describe('shellQuote', () => {
  it('neutralises embedded quotes and shell syntax', () => {
    expect(shellQuote(`x' ; rm -rf / ; echo '`)).toBe(`'x'\\'' ; rm -rf / ; echo '\\'''`)
  })
})
