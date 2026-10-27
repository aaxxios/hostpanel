/**
 * Unit tests for ServerSshSettingsModal: it loads the server's settings, never shows a stored
 * private key, sends an empty key field as "keep", shows field refusals, runs the health check and
 * flags an unsupported module.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import ServerSshSettingsModal from '@/components/servers/ServerSshSettingsModal.vue'
import { callOf, invalid, json, stubFetch } from '../../helpers/fetch'
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

/**
 * Mounts the dialog for server 4 and waits for its load.
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

  it('loads the settings into the form and leaves the private key field empty', async () => {
    const mock = stubFetch(json(SETTINGS))
    const wrapper = await mountDialog()

    expect(callOf(mock).url).toContain('/api/admin/servers/4/ssh-settings')
    expect((wrapper.get('[data-testid="server-ssh-hostname"]').element as HTMLInputElement).value).toBe('cwp1.example.test')
    expect((wrapper.get('[data-testid="server-ssh-private-key"]').element as HTMLTextAreaElement).value).toBe('')
    expect(wrapper.get('[data-testid="server-ssh-health"]').text()).toBe('Not tested')
  })

  it('sends an empty private key field as null, so the stored key is kept', async () => {
    const mock = stubFetch(json(SETTINGS), json({ ...SETTINGS, isEnabled: true }))
    const wrapper = await mountDialog()

    await wrapper.get('[data-testid="server-ssh-enabled"]').trigger('click')
    await wrapper.get('[data-testid="server-ssh-save"]').trigger('click')
    await flushPromises()

    const body = callOf(mock, 1).body as Record<string, unknown>
    expect(body.isEnabled).toBe(true)
    expect(body.agentPrivateKey).toBeNull()
    expect(wrapper.emitted('close')).toHaveLength(1)
  })

  it('shows the refusal and the field sentence, and stays open', async () => {
    stubFetch(json(SETTINGS), invalid('Validation failed.', { agentHostKeyFingerprint: ['Bad fingerprint.'] }))
    const wrapper = await mountDialog()

    await wrapper.get('[data-testid="server-ssh-fingerprint"]').setValue('md5:nope')
    await wrapper.get('[data-testid="server-ssh-save"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="server-ssh-error"]').text()).toContain('Validation failed.')
    expect(wrapper.text()).toContain('Bad fingerprint.')
    expect(wrapper.emitted('close')).toBeUndefined()
  })

  it('runs the health check and shows the agent version', async () => {
    stubFetch(json(SETTINGS), json({ isHealthy: true, agentVersion: '1.0.0', error: null }), json({ ...SETTINGS, isHealthy: true }))
    const wrapper = await mountDialog()

    await wrapper.get('[data-testid="server-ssh-test"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="server-ssh-health"]').text()).toBe('Agent reachable')
    expect(wrapper.get('[data-testid="server-ssh-agent-version"]').text()).toContain('1.0.0')
  })

  it('warns that an unsupported module cannot be enabled', async () => {
    stubFetch(json({ ...SETTINGS, isSupported: false, hasAgentPrivateKey: false }))
    const wrapper = await mountDialog()

    expect(wrapper.find('[data-testid="server-ssh-unsupported"]').exists()).toBe(true)
    expect(wrapper.get('[data-testid="server-ssh-test"]').attributes('disabled')).toBeDefined()
  })
})
