/**
 * Unit tests for the guided SSH setup dialog: it loads settings and setup, never shows a stored
 * private key, generates an identity and shows a hash-verifying installer command, reads and pins a
 * host key, flags an agent installed for another server, and keeps an empty key field as "keep".
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import ServerSshSettingsModal from '@/components/servers/ServerSshSettingsModal.vue'
import { callOf, invalid, json, problem, stubFetch } from '../../helpers/fetch'
import { mountWithApp } from '../../helpers/mount'

/** Settings as the API answers them. */
const SETTINGS = {
  serverId: 4,
  isSupported: true,
  isEnabled: false,
  sshHostname: 'cwp1.example.test',
  sshPort: 2222,
  agentHost: 'cwp1.internal.test',
  agentPort: 22,
  agentUsername: 'hpsshkeys',
  hasAgentPrivateKey: true,
  agentHostKeyFingerprint: 'SHA256:p6R0QgF93KJCG8T5VLbbvfWcFGdwzv2ViKOejXV5TLw',
  connectTimeoutSeconds: 10,
  operationTimeoutSeconds: 20,
  isHealthy: null,
  lastHealthCheckAt: null,
}

/** Setup before an identity exists. */
const SETUP = {
  serverId: 4,
  profile: 'cwp',
  publicKey: null,
  binding: null,
  agentUsername: 'hpsshkeys',
  bundleVersion: '1.1.0',
  bundleSha256: 'b'.repeat(64),
  bundlePath: '/api/public/ssh-agent/bundle.tar.gz',
  sourceAddresses: ['203.0.113.10'],
}

/** Setup after an identity was generated. */
const SETUP_WITH_KEY = { ...SETUP, publicKey: 'ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b hostpanel-server-4', binding: '22222222-2222-4222-8222-222222222222' }

/**
 * Mounts the dialog for server 4 and waits for its two loads.
 *
 * @returns The mounted dialog.
 */
const mountDialog = async (): Promise<ReturnType<typeof mountWithApp>> => {
  const wrapper = mountWithApp(ServerSshSettingsModal, { props: { serverId: 4, serverName: 'cwp1' } })
  await flushPromises()
  return wrapper
}

describe('ServerSshSettingsModal', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('loads settings and setup, and leaves the private key field empty', async () => {
    const mock = stubFetch(json(SETTINGS), json(SETUP))
    const wrapper = await mountDialog()

    expect(callOf(mock, 0).url).toContain('/api/admin/servers/4/ssh-settings')
    expect(callOf(mock, 1).url).toContain('/api/admin/servers/4/ssh-setup')
    expect((wrapper.get('[data-testid="server-ssh-hostname"]').element as HTMLInputElement).value).toBe('cwp1.example.test')
    expect((wrapper.get('[data-testid="server-ssh-private-key"]').element as HTMLTextAreaElement).value).toBe('')
    expect(wrapper.find('[data-testid="server-ssh-install-command"]').exists()).toBe(false)
    expect(wrapper.get('[data-testid="server-ssh-generate"]').text()).toBe('Generate identity')
  })

  it('generates an identity and shows the hash-verifying installer command', async () => {
    const mock = stubFetch(json(SETTINGS), json(SETUP), json(SETUP_WITH_KEY), json(SETTINGS))
    const wrapper = await mountDialog()

    await wrapper.get('[data-testid="server-ssh-generate"]').trigger('click')
    await flushPromises()

    expect(callOf(mock, 2).method).toBe('POST')
    expect(callOf(mock, 2).url).toContain('/ssh-identity')
    const command = wrapper.get('[data-testid="server-ssh-install-command"]').text()
    expect(command).toContain(`sha256sum -c -`)
    expect(command).toContain('b'.repeat(64))
    expect(command).toContain(`--provider 'cwp' --binding '${SETUP_WITH_KEY.binding}'`)
    expect(command).not.toContain('PRIVATE')
    expect(wrapper.get('[data-testid="server-ssh-generate"]').text()).toBe('Rotate identity')
  })

  it('reads the host key and pins it only through the form', async () => {
    const mock = stubFetch(json(SETTINGS), json(SETUP), json({ algorithm: 'ssh-ed25519', fingerprint: 'SHA256:' + 'Z'.repeat(43), matchesPinned: false }), json(SETTINGS))
    const wrapper = await mountDialog()

    await wrapper.get('[data-testid="server-ssh-scan"]').trigger('click')
    await flushPromises()
    expect(wrapper.get('[data-testid="server-ssh-scan-match"]').text()).toBe('Not the pinned key')
    await wrapper.get('[data-testid="server-ssh-use-scanned"]').trigger('click')
    // The badge follows the fingerprint field, not the answer the scan gave before it was used.
    expect(wrapper.get('[data-testid="server-ssh-scan-match"]').text()).toBe('Matches the pinned key')
    expect(wrapper.find('[data-testid="server-ssh-use-scanned"]').exists()).toBe(false)
    await wrapper.get('[data-testid="server-ssh-save"]').trigger('click')
    await flushPromises()

    expect(callOf(mock, 2).url).toContain('/ssh-settings/scan-host-key')
    expect((callOf(mock, 3).body as Record<string, unknown>).agentHostKeyFingerprint).toBe('SHA256:' + 'Z'.repeat(43))
  })

  it('shows the firewall and sshd lines that let only this HostPanel in on the agent port', async () => {
    stubFetch(json(SETTINGS), json(SETUP))
    const wrapper = await mountDialog()

    const network = wrapper.get('[data-testid="server-ssh-network"]').text()
    expect(network).toContain(`csf -a 203.0.113.10 'HostPanel SSH key agent'`)
    expect(network).toContain(`ufw allow from 203.0.113.10 to any port ${SETTINGS.agentPort} proto tcp`)
    expect(network).toContain('PerSourcePenaltyExemptList 203.0.113.10')
  })

  it('asks for HostPanel\'s address rather than guessing one', async () => {
    stubFetch(json(SETTINGS), json({ ...SETUP, sourceAddresses: [] }))
    const wrapper = await mountDialog()

    expect(wrapper.find('[data-testid="server-ssh-network-lines"]').exists()).toBe(false)
    expect(wrapper.get('[data-testid="server-ssh-network-unset"]').text()).toContain('SshAgent__SourceAddresses')
  })

  it('shows a failed host key read beside the button, not at the top of the dialog', async () => {
    stubFetch(json(SETTINGS), json(SETUP), problem('No answer from the agent host and port.', 503))
    const wrapper = await mountDialog()

    await wrapper.get('[data-testid="server-ssh-scan"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="server-ssh-scan-error"]').text()).toContain('No answer from the agent host and port.')
    expect(wrapper.find('[data-testid="server-ssh-error"]').exists()).toBe(false)
  })

  it('explains an agent installed for another server', async () => {
    stubFetch(json(SETTINGS), json(SETUP), json({ isHealthy: false, agentVersion: '1.1.0', error: 'binding_mismatch', provider: 'cwp' }), json({ ...SETTINGS, isHealthy: false }))
    const wrapper = await mountDialog()

    await wrapper.get('[data-testid="server-ssh-test"]').trigger('click')
    await flushPromises()

    expect(wrapper.find('[data-testid="server-ssh-binding-mismatch"]').exists()).toBe(true)
    expect(wrapper.get('[data-testid="server-ssh-health"]').text()).toBe('Agent unreachable')
  })

  it('runs the health check and shows version and provider', async () => {
    stubFetch(json(SETTINGS), json(SETUP), json({ isHealthy: true, agentVersion: '1.1.0', error: null, provider: 'cwp' }), json({ ...SETTINGS, isHealthy: true }))
    const wrapper = await mountDialog()

    await wrapper.get('[data-testid="server-ssh-test"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="server-ssh-health"]').text()).toBe('Agent reachable')
    expect(wrapper.get('[data-testid="server-ssh-agent-version"]').text()).toBe('Agent 1.1.0 (cwp)')
  })

  it('sends an empty private key field as null, and shows a refused enable', async () => {
    const mock = stubFetch(json(SETTINGS), json(SETUP), invalid('Run a successful agent test with these settings before enabling SSH key management.', {}))
    const wrapper = await mountDialog()

    await wrapper.get('[data-testid="server-ssh-enabled"]').trigger('click')
    await wrapper.get('[data-testid="server-ssh-save"]').trigger('click')
    await flushPromises()

    const body = callOf(mock, 2).body as Record<string, unknown>
    expect(body.isEnabled).toBe(true)
    expect(body.agentPrivateKey).toBeNull()
    expect(wrapper.get('[data-testid="server-ssh-error"]').text()).toContain('successful agent test')
    expect(wrapper.emitted('close')).toBeUndefined()
  })

  it('warns that an unsupported module has no installer', async () => {
    stubFetch(json({ ...SETTINGS, isSupported: false, hasAgentPrivateKey: false }), json({ ...SETUP, profile: null }))
    const wrapper = await mountDialog()

    expect(wrapper.find('[data-testid="server-ssh-unsupported"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="server-ssh-generate"]').exists()).toBe(false)
    expect(wrapper.get('[data-testid="server-ssh-test"]').attributes('disabled')).toBeDefined()
  })
})
