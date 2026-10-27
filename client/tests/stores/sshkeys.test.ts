/**
 * Unit tests for the SSH key store: what loads when, what a refusal leaves behind, and that a
 * private key never leaves the browser.
 */
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { computed, ref } from 'vue'
import { ActionOutcome } from '~/types/actionoutcome'
import { SshAccessState } from '~/types/sshaccessstate'
import { SshKeyLocalError } from '~/types/sshkeylocalerror'
import { SshKeySyncState } from '~/types/sshkeysyncstate'

/** The mocked SSH endpoints, reset before every test. */
const api = vi.hoisted(() => ({
  fetchSshAccess: vi.fn(),
  fetchSshKeys: vi.fn(),
  addSshKey: vi.fn(),
  revokeSshKey: vi.fn()
}))

vi.stubGlobal('ref', ref)
vi.stubGlobal('computed', computed)

vi.mock('~/composables/apis/useClientApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns The mocked endpoints.
   */
  useClientApi: () => api
}))

const { useSshKeysStore } = await import('~/stores/sshkeys')

/** A real ED25519 public key line. */
const KEY = 'ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b me@host'

/**
 * An access summary in the given state.
 *
 * @param state - The access state.
 * @returns The summary.
 */
const access = (state: SshAccessState): Record<string, unknown> =>
  ({ state, hostname: 'h', port: 22, username: 'web', connectCommand: null, maxKeys: 2 })

/**
 * A listed key.
 *
 * @param keyId - Its id.
 * @returns The key.
 */
const key = (keyId: string): Record<string, unknown> =>
  ({ keyId, label: 'l', fingerprint: 'SHA256:x', createdAt: '', syncState: SshKeySyncState.Installed, isStale: false, presentOnServer: true })

/**
 * A refusal as `apiFetch` throws it.
 *
 * @param detail - The API's sentence.
 * @returns The error.
 */
const refusal = (detail: string): Record<string, unknown> => ({ data: { detail } })

describe('useSshKeysStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    Object.values(api).forEach((fn) => fn.mockReset())
  })

  it('loads access and keys for exactly the given service', async () => {
    api.fetchSshAccess.mockResolvedValue(access(SshAccessState.Available))
    api.fetchSshKeys.mockResolvedValue([key('a')])
    const store = useSshKeysStore()

    const pending = store.load('42')
    expect(store.loading).toBe(true)
    await pending

    expect(api.fetchSshAccess).toHaveBeenCalledWith('42')
    expect(api.fetchSshKeys).toHaveBeenCalledWith('42')
    expect(store.loading).toBe(false)
    expect(store.canManage).toBe(true)
    expect(store.keys).toHaveLength(1)
  })

  it('skips the key list for an inactive service', async () => {
    api.fetchSshAccess.mockResolvedValue(access(SshAccessState.ServiceInactive))
    const store = useSshKeysStore()
    await store.load('42')
    expect(api.fetchSshKeys).not.toHaveBeenCalled()
    expect(store.canManage).toBe(false)
    expect(store.canRevoke).toBe(false)
  })

  it('cannot manage in any state but available', async () => {
    for (const state of [SshAccessState.Unsupported, SshAccessState.ShellDisabled, SshAccessState.NotConfigured, SshAccessState.Unreachable]) {
      api.fetchSshAccess.mockResolvedValue(access(state))
      api.fetchSshKeys.mockResolvedValue([])
      const store = useSshKeysStore()
      await store.load('1')
      expect(store.canManage).toBe(false)
      expect(store.canRevoke).toBe(true)
    }
  })

  it('keeps the API sentence when loading fails', async () => {
    api.fetchSshAccess.mockRejectedValue(refusal('No such client service.'))
    const store = useSshKeysStore()
    await store.load('9')
    expect(store.loadError).toBe('No such client service.')
    expect(store.loading).toBe(false)
  })

  it('never sends a private key', async () => {
    const store = useSshKeysStore()
    expect(await store.add('1', 'x', '-----BEGIN OPENSSH PRIVATE KEY-----\nabc')).toBe(ActionOutcome.Failed)
    expect(store.localError).toBe(SshKeyLocalError.PrivateKey)
    expect(api.addSshKey).not.toHaveBeenCalled()
  })

  it('refuses a malformed key or an empty label locally', async () => {
    const store = useSshKeysStore()
    expect(await store.add('1', 'x', 'ssh-rsa AAAA')).toBe(ActionOutcome.Failed)
    expect(store.localError).toBe(SshKeyLocalError.InvalidKey)
    expect(await store.add('1', '  ', KEY)).toBe(ActionOutcome.Failed)
    expect(store.localError).toBe(SshKeyLocalError.LabelRequired)
    expect(api.addSshKey).not.toHaveBeenCalled()
  })

  it('adds a key, trimmed, and appends it', async () => {
    api.addSshKey.mockResolvedValue(key('new'))
    const store = useSshKeysStore()
    expect(await store.add('7', ' laptop ', KEY)).toBe(ActionOutcome.Succeeded)
    expect(api.addSshKey).toHaveBeenCalledWith('7', { label: 'laptop', publicKey: KEY })
    expect(store.keys.map((k) => k.keyId)).toEqual(['new'])
  })

  it('shows the API sentence and refreshes the list when adding fails', async () => {
    api.addSshKey.mockRejectedValue(refusal('The hosting server could not be reached.'))
    api.fetchSshKeys.mockResolvedValue([{ ...key('k'), syncState: SshKeySyncState.InstallFailed }])
    const store = useSshKeysStore()
    expect(await store.add('7', 'laptop', KEY)).toBe(ActionOutcome.Failed)
    expect(store.addError).toBe('The hosting server could not be reached.')
    expect(store.keys[0]?.syncState).toBe(SshKeySyncState.InstallFailed)
    expect(store.adding).toBe(false)
  })

  it('revokes a key and removes it from the list', async () => {
    api.fetchSshAccess.mockResolvedValue(access(SshAccessState.Available))
    api.fetchSshKeys.mockResolvedValue([key('a'), key('b')])
    api.revokeSshKey.mockResolvedValue(undefined)
    const store = useSshKeysStore()
    await store.load('7')
    expect(await store.revoke('7', 'a')).toBe(ActionOutcome.Succeeded)
    expect(api.revokeSshKey).toHaveBeenCalledWith('7', 'a')
    expect(store.keys.map((k) => k.keyId)).toEqual(['b'])
    expect(store.revokingId).toBeNull()
  })

  it('keeps the key and the sentence when revoking fails', async () => {
    api.fetchSshKeys.mockResolvedValue([key('a')])
    api.revokeSshKey.mockRejectedValue(refusal('Unavailable'))
    const store = useSshKeysStore()
    expect(await store.revoke('7', 'a')).toBe(ActionOutcome.Failed)
    expect(store.revokeError).toBe('Unavailable')
    expect(store.keys).toHaveLength(1)
  })

  it('reports the key limit and clears everything on reset', async () => {
    api.fetchSshAccess.mockResolvedValue(access(SshAccessState.Available))
    api.fetchSshKeys.mockResolvedValue([key('a'), key('b')])
    const store = useSshKeysStore()
    await store.load('7')
    expect(store.atLimit).toBe(true)
    store.reset()
    expect(store.access).toBeNull()
    expect(store.keys).toEqual([])
  })
})
