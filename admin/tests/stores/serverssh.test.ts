/**
 * Unit tests for the server SSH settings store: load, save with field errors, the health check,
 * and that a stored private key is never read back.
 */
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { useServerSshStore } from '@/stores/serverssh'
import { callOf, invalid, json, problem, stubFetch } from '../helpers/fetch'

/** Settings as the API answers them. */
const SETTINGS = {
  serverId: 4,
  isSupported: true,
  isEnabled: true,
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

describe('serverSshStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('loads one server\'s settings from its own path', async () => {
    const mock = stubFetch(json(SETTINGS))
    const store = useServerSshStore()

    await store.load(4)

    expect(callOf(mock).url).toContain('/api/admin/servers/4/ssh-settings')
    expect(store.settings).toEqual(SETTINGS)
    expect(store.loading).toBe(false)
  })

  it('saves with PUT and keeps the answer', async () => {
    const mock = stubFetch(json({ ...SETTINGS, sshPort: 22 }))
    const store = useServerSshStore()

    expect(await store.save(4, { ...SETTINGS, agentPrivateKey: null } as never)).toBe(true)

    const call = callOf(mock)
    expect(call.method).toBe('PUT')
    expect((call.body as Record<string, unknown>).agentPrivateKey).toBeNull()
    expect(store.settings?.sshPort).toBe(22)
  })

  it('keeps the field sentences of a refused save', async () => {
    stubFetch(invalid('Validation failed.', { agentHostKeyFingerprint: ['The host key fingerprint must look like SHA256:…'] }))
    const store = useServerSshStore()

    expect(await store.save(4, SETTINGS as never)).toBe(false)
    expect(store.error).toBe('Validation failed.')
    expect(store.fieldErrors.agentHostKeyFingerprint).toContain('SHA256')
  })

  it('runs the health check, then reloads to show the recorded outcome', async () => {
    const mock = stubFetch(json({ isHealthy: false, agentVersion: null, error: 'unreachable' }), json({ ...SETTINGS, isHealthy: false }))
    const store = useServerSshStore()

    await store.test(4)

    expect(callOf(mock, 0).method).toBe('POST')
    expect(callOf(mock, 0).url).toContain('/ssh-settings/test')
    expect(store.lastTest?.error).toBe('unreachable')
    expect(store.settings?.isHealthy).toBe(false)
    expect(store.testing).toBe(false)
  })

  it('keeps the API sentence when the check is refused, and reset clears everything', async () => {
    stubFetch(problem('The SSH settings for this server are incomplete.'))
    const store = useServerSshStore()

    await store.test(4)
    expect(store.error).toBe('The SSH settings for this server are incomplete.')
    expect(store.lastTest).toBeNull()

    store.reset()
    expect(store.error).toBeNull()
    expect(store.settings).toBeNull()
  })

  it('loads settings and setup together, generates an identity and scans the host key', async () => {
    const setup = { serverId: 4, profile: 'cwp', publicKey: null, binding: null, agentUsername: 'hpsshkeys', bundleVersion: '1.1.0', bundleSha256: 'c'.repeat(64), bundlePath: '/api/public/ssh-agent/bundle.tar.gz' }
    const generated = { ...setup, publicKey: 'ssh-ed25519 AAAA x', binding: '33333333-3333-4333-8333-333333333333' }
    const mock = stubFetch(json(SETTINGS), json(setup), json(generated), json({ ...SETTINGS, isEnabled: false }), json({ algorithm: 'ssh-ed25519', fingerprint: 'SHA256:x', matchesPinned: false }))
    const store = useServerSshStore()

    await store.loadAll(4)
    expect(store.setup?.profile).toBe('cwp')
    expect(await store.generateIdentity(4)).toBe(true)
    expect(callOf(mock, 2).url).toContain('/api/admin/servers/4/ssh-identity')
    expect(store.setup?.binding).toBe(generated.binding)
    await store.scanHostKey(4)
    expect(store.lastScan?.fingerprint).toBe('SHA256:x')
    expect(store.generating).toBe(false)
    expect(store.scanning).toBe(false)
  })

  it('keeps the sentence of a refused identity generation', async () => {
    stubFetch(problem('SSH key management is not supported for this server type.'))
    const store = useServerSshStore()

    expect(await store.generateIdentity(4)).toBe(false)
    expect(store.error).toBe('SSH key management is not supported for this server type.')
  })
})
