/**
 * Unit tests for ServiceSshKeys: each access state, the list, the add form's local checks and
 * the revoke confirmation, through the real store with the endpoints mocked.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { mountClient } from '~/tests/helpers/mountClient'
import ServiceSshKeys from '~/components/client/services/ServiceSshKeys.vue'
import { SshAccessState } from '~/types/sshaccessstate'
import { SshKeySyncState } from '~/types/sshkeysyncstate'

/** The mocked client endpoints the store reaches. */
const api = vi.hoisted(() => ({
  fetchSshAccess: vi.fn(),
  fetchSshKeys: vi.fn(),
  addSshKey: vi.fn(),
  revokeSshKey: vi.fn()
}))

vi.mock('~/composables/apis/useClientApi', () => ({ useClientApi: () => api }))

/** A real ED25519 public key line. */
const KEY = 'ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b me@host'

/**
 * Mounts the panel for service 6 in the given access state with the given keys.
 *
 * @param state - The access state the API answers.
 * @param keys - The keys the API lists.
 * @returns The mounted panel, once loaded.
 */
const mountIn = async (state: SshAccessState, keys: Record<string, unknown>[] = []): Promise<ReturnType<typeof mountClient>> => {
  api.fetchSshAccess.mockResolvedValue({
    state,
    hostname: 'cwp.example.test',
    port: 2222,
    username: 'web',
    connectCommand: state === SshAccessState.Available ? 'ssh -p 2222 web@cwp.example.test' : null,
    maxKeys: 20
  })
  api.fetchSshKeys.mockResolvedValue(keys)
  const wrapper = mountClient(ServiceSshKeys, { props: { serviceId: '6' } })
  await vi.waitFor(() => expect(wrapper.find('[data-testid="service-ssh-loading"]').exists()).toBe(false))
  await wrapper.vm.$nextTick()
  return wrapper
}

/** One installed key. */
const KEY_ROW = { keyId: 'k1', label: 'laptop', fingerprint: 'SHA256:abc', createdAt: '2026-10-27T00:00:00Z', syncState: SshKeySyncState.Installed, isStale: false, presentOnServer: true }

describe('ServiceSshKeys', () => {
  beforeEach(() => {
    Object.values(api).forEach((fn) => fn.mockReset())
  })

  it('loads this service and shows the connect command and the empty state', async () => {
    const wrapper = await mountIn(SshAccessState.Available)

    expect(api.fetchSshAccess).toHaveBeenCalledWith('6')
    expect(wrapper.find('[data-testid="service-ssh-command"]').text()).toBe('ssh -p 2222 web@cwp.example.test')
    expect(wrapper.find('[data-testid="service-ssh-empty"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="service-ssh-form"]').exists()).toBe(true)
  })

  it('lets keys be revoked when SSH was switched off on the server, but offers no form', async () => {
    const wrapper = await mountIn(SshAccessState.NotConfigured, [KEY_ROW])

    expect(wrapper.find('[data-testid="service-ssh-revoke"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="service-ssh-form"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="service-ssh-connection"]').exists()).toBe(false)
  })

  it('flags a key still off the server just after the service came back', async () => {
    const wrapper = await mountIn(SshAccessState.Available, [{ ...KEY_ROW, syncState: SshKeySyncState.Suspended, presentOnServer: false }])

    expect(wrapper.find('[data-testid="service-ssh-key-badge"]').text()).toBe('client.services.ssh.sync.Suspended')
    expect(wrapper.find('[data-testid="service-ssh-revoke"]').exists()).toBe(true)
  })

  it('offers no revoke for an inactive service', async () => {
    const wrapper = await mountIn(SshAccessState.ServiceInactive)

    expect(wrapper.find('[data-testid="service-ssh-revoke"]').exists()).toBe(false)
  })

  it.each([SshAccessState.ShellDisabled, SshAccessState.Unsupported, SshAccessState.NotConfigured, SshAccessState.Unreachable])(
    'explains the %s state and offers no form', async (state) => {
      const wrapper = await mountIn(state)

      expect(wrapper.find('[data-testid="service-ssh-state"]').text()).toContain(`client.services.ssh.state.${state}`)
      expect(wrapper.find('[data-testid="service-ssh-form"]').exists()).toBe(false)
      expect(wrapper.find('[data-testid="service-ssh-command"]').exists()).toBe(false)
      // Nothing that suggests a login or a form the customer cannot use.
      expect(wrapper.find('[data-testid="service-ssh-connection"]').exists()).toBe(false)
      expect(wrapper.find('[data-testid="service-ssh-empty"]').exists()).toBe(false)
      expect(wrapper.find('[data-testid="service-ssh-keys"]').exists()).toBe(false)
      expect(wrapper.find('[data-testid="service-ssh-howto"]').exists()).toBe(false)
    })

  it('shows a load failure with a retry', async () => {
    api.fetchSshAccess.mockRejectedValue({ data: { detail: 'No such client service.' } })
    const wrapper = mountClient(ServiceSshKeys, { props: { serviceId: '6' } })

    await vi.waitFor(() => expect(wrapper.find('[data-testid="service-ssh-load-error"]').exists()).toBe(true))

    expect(wrapper.find('[data-testid="service-ssh-load-error"]').text()).toContain('No such client service.')
    expect(wrapper.find('[data-testid="service-ssh-retry"]').exists()).toBe(true)
  })

  it('lists keys with fingerprint and flags a stale one, which can still be revoked', async () => {
    const wrapper = await mountIn(SshAccessState.Available, [KEY_ROW, { ...KEY_ROW, keyId: 'k2', label: 'old', isStale: true }])

    const rows = wrapper.findAll('[data-testid="service-ssh-key"]')
    expect(rows).toHaveLength(2)
    expect(rows[0]?.find('[data-testid="service-ssh-key-fingerprint"]').text()).toBe('SHA256:abc')
    expect(rows[0]?.find('[data-testid="service-ssh-revoke"]').exists()).toBe(true)
    expect(rows[1]?.find('[data-testid="service-ssh-key-badge"]').text()).toContain('client.services.ssh.sync.Stale')
    expect(rows[1]?.find('[data-testid="service-ssh-revoke"]').exists()).toBe(true)
  })

  it('refuses a private key in the browser', async () => {
    const wrapper = await mountIn(SshAccessState.Available)

    await wrapper.find('[data-testid="service-ssh-public-key"]').setValue('-----BEGIN OPENSSH PRIVATE KEY-----\nabc')
    await wrapper.find('[data-testid="service-ssh-label"]').setValue('x')
    await wrapper.find('form').trigger('submit')

    expect(wrapper.find('[data-testid="ui-form-error"]').text()).toContain('client.services.ssh.localError.privateKey')
    expect(api.addSshKey).not.toHaveBeenCalled()
  })

  it('pre-fills the label from the key comment and adds the key', async () => {
    api.addSshKey.mockResolvedValue({ ...KEY_ROW, keyId: 'new', label: 'me@host' })
    const wrapper = await mountIn(SshAccessState.Available)

    await wrapper.find('[data-testid="service-ssh-public-key"]').setValue(KEY)
    expect((wrapper.find('[data-testid="service-ssh-label"]').element as HTMLInputElement).value).toBe('me@host')
    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(wrapper.findAll('[data-testid="service-ssh-key"]')).toHaveLength(1))
    expect(api.addSshKey).toHaveBeenCalledWith('6', { label: 'me@host', publicKey: KEY })
  })

  it('shows the API refusal above the form', async () => {
    api.addSshKey.mockRejectedValue({ data: { detail: 'This key is already installed on the service.' } })
    const wrapper = await mountIn(SshAccessState.Available)

    await wrapper.find('[data-testid="service-ssh-public-key"]').setValue(KEY)
    await wrapper.find('form').trigger('submit')

    await vi.waitFor(() => expect(wrapper.find('[data-testid="ui-form-error"]').text()).toContain('already installed'))
  })

  it('revokes only after confirmation', async () => {
    api.revokeSshKey.mockResolvedValue(undefined)
    const wrapper = await mountIn(SshAccessState.Available, [KEY_ROW])

    await wrapper.find('[data-testid="service-ssh-revoke"]').trigger('click')
    expect(api.revokeSshKey).not.toHaveBeenCalled()
    expect(wrapper.find('[data-testid="ui-confirm-dialog"]').text()).toContain('client.services.ssh.revokeConfirmTitle')
    await wrapper.find('[data-testid="ui-confirm-confirm"]').trigger('click')

    await vi.waitFor(() => expect(api.revokeSshKey).toHaveBeenCalledWith('6', 'k1'))
    await vi.waitFor(() => expect(wrapper.findAll('[data-testid="service-ssh-key"]')).toHaveLength(0))
  })
})
