/**
 * Mount tests for ServersView: servers grouped by module, the groups table, the connection test
 * result in the row, the delete confirmation, and the empty and refused states.
 */
import { afterEach, describe, expect, it, vi } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import ServersView from '@/views/ServersView.vue'
import { mountRouted } from '../helpers/mount'
import { stubRoutes } from '../helpers/routes'
import { json, problem } from '../helpers/fetch'
import { serverDto, serverGroupDto } from '../helpers/serverFixtures'

/**
 * Mounts the screen and waits for the first load.
 *
 * @returns The wrapper.
 */
const mountServers = async () => {
  const { wrapper } = await mountRouted(ServersView, '/servers', '/servers')
  await flushPromises()
  return wrapper
}

describe('ServersView', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('lists servers under their module with state and usage', async () => {
    stubRoutes({
      'GET /api/admin/servers': () => json([serverDto()]),
      'GET /api/admin/server-groups': () => json([serverGroupDto()]),
    })
    const wrapper = await mountServers()

    const row = wrapper.get('[data-testid="server-row-1"]')
    expect(row.text()).toContain('host1')
    expect(row.text()).toContain('12 / 100')
    expect(wrapper.get('[data-testid="server-state-1"]').text()).toBe('Online')
    expect(wrapper.find('[data-testid="servers-table-Cwp7"]').exists()).toBe(true)
  })

  it('lists the groups with their fill type and servers', async () => {
    stubRoutes({
      'GET /api/admin/servers': () => json([serverDto()]),
      'GET /api/admin/server-groups': () => json([serverGroupDto()]),
    })
    const wrapper = await mountServers()

    const row = wrapper.get('[data-testid="server-group-row-3"]')
    expect(row.text()).toContain('Shared')
    expect(row.text()).toContain('host1')
  })

  it('shows the connection test result in the row', async () => {
    stubRoutes({
      'GET /api/admin/servers': () => json([serverDto()]),
      'GET /api/admin/server-groups': () => json([]),
      'POST /api/admin/servers/1/test-connection': () =>
        json({ connected: true, accountsCount: 14, version: '7.1', errorMessage: null, testedAt: '2026-10-14T00:00:00Z' }),
    })
    const wrapper = await mountServers()

    await wrapper.get('[data-testid="server-test-1"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="server-test-result-1"]').text()).toContain('14')
  })

  it('shows the API sentence when the connection test is refused', async () => {
    stubRoutes({
      'GET /api/admin/servers': () => json([serverDto()]),
      'GET /api/admin/server-groups': () => json([]),
      'POST /api/admin/servers/1/test-connection': () => problem('Credentials were rejected.', 400),
    })
    const wrapper = await mountServers()

    await wrapper.get('[data-testid="server-test-1"]').trigger('click')
    await flushPromises()

    expect(wrapper.get('[data-testid="server-test-result-1"]').text()).toContain('Credentials were rejected.')
  })

  it('asks for confirmation before deleting a server', async () => {
    const mock = stubRoutes({
      'GET /api/admin/servers': () => json([serverDto()]),
      'GET /api/admin/server-groups': () => json([]),
      'DELETE /api/admin/servers/1': () => new Response(null, { status: 204 }),
    })
    const wrapper = await mountServers()

    await wrapper.get('[data-testid="server-delete-1"]').trigger('click')
    expect(mock.mock.calls.some(([, init]) => init?.method === 'DELETE')).toBe(false)

    await wrapper.get('[data-testid="dialog-confirm"]').trigger('click')
    await flushPromises()

    expect(mock.mock.calls.some(([url, init]) => String(url) === '/api/admin/servers/1' && init?.method === 'DELETE')).toBe(true)
  })

  it('shows the empty states when there are no servers or groups', async () => {
    stubRoutes({
      'GET /api/admin/servers': () => json([]),
      'GET /api/admin/server-groups': () => json([]),
    })
    const wrapper = await mountServers()

    expect(wrapper.find('[data-testid="servers-empty"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="server-groups-empty"]').exists()).toBe(true)
  })

  it('opens the server dialog from the add button', async () => {
    stubRoutes({
      'GET /api/admin/servers': () => json([]),
      'GET /api/admin/server-groups': () => json([]),
    })
    const wrapper = await mountServers()

    await wrapper.get('[data-testid="servers-add"]').trigger('click')

    expect(wrapper.find('[data-testid="server-form-name"]').exists()).toBe(true)
  })

  it('opens the SSH key management dialog for the row\'s server', async () => {
    stubRoutes({
      'GET /api/admin/servers': () => json([serverDto()]),
      'GET /api/admin/server-groups': () => json([]),
      'GET /api/admin/servers/1/ssh-settings': () =>
        json({
          serverId: 1, isSupported: true, isEnabled: false, sshHostname: '', sshPort: 22, agentHost: '', agentPort: 22,
          agentUsername: '', hasAgentPrivateKey: false, agentHostKeyFingerprint: '', connectTimeoutSeconds: 10,
          operationTimeoutSeconds: 20, isHealthy: null, lastHealthCheckAt: null,
        }),
      'GET /api/admin/servers/1/ssh-setup': () =>
        json({
          serverId: 1, profile: 'cwp', publicKey: null, binding: null, agentUsername: 'hpsshkeys',
          bundleVersion: '1.1.0', bundleSha256: 'd'.repeat(64), bundlePath: '/api/public/ssh-agent/bundle.tar.gz', sourceAddresses: [],
        }),
    })
    const wrapper = await mountServers()

    await wrapper.get('[data-testid="server-ssh-1"]').trigger('click')
    await flushPromises()

    expect(wrapper.find('[data-testid="server-ssh-hostname"]').exists()).toBe(true)
    await wrapper.get('[data-testid="server-ssh-cancel"]').trigger('click')
    expect(wrapper.find('[data-testid="server-ssh-hostname"]').exists()).toBe(false)
  })
})
