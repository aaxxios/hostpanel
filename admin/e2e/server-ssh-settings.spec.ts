/**
 * Server SSH key management setup: opening the guided dialog from a server row, generating a
 * per-server identity and its hash-verifying installer command, saving with the stored key kept
 * (never read back), and the agent health check including an agent bound to another server.
 */
import { HttpMethod } from '../src/types/http'
import { expect, test } from '@playwright/test'
import { hostServer, hostServerSsh } from './fixtures/servers'
import type { ServerSshSettingsDto, ServerSshSetupDto } from '../src/types/serverssh'
import { json, stubApi } from './support/stubApi'

/** The settings path of the fixture server. */
const SSH_PATH = `/api/admin/servers/${hostServer.id}/ssh-settings`

/** The setup path of the fixture server. */
const SETUP_PATH = `/api/admin/servers/${hostServer.id}/ssh-setup`

/** Setup before any identity exists. */
const SETUP: ServerSshSetupDto = {
  serverId: hostServer.id,
  profile: 'cwp',
  publicKey: null,
  binding: null,
  agentUsername: 'hpsshkeys',
  bundleVersion: '1.1.0',
  bundleSha256: 'e'.repeat(64),
  bundlePath: '/api/public/ssh-agent/bundle.tar.gz',
}

test('generating an identity shows an installer command that verifies the bundle hash', async ({ page }) => {
  const generated: ServerSshSetupDto = { ...SETUP, publicKey: 'ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIG6EJIHRxsxge2aZnl+clPBfoX0nmvD7FfHXM2QQ3+3b hostpanel-server-1', binding: '44444444-4444-4444-8444-444444444444' }
  await stubApi(page, {
    signedIn: true,
    routes: [
      json(HttpMethod.Get, '/api/admin/servers', [hostServer]),
      json(HttpMethod.Get, '/api/admin/server-groups', []),
      json(HttpMethod.Get, SSH_PATH, hostServerSsh),
      json(HttpMethod.Get, SETUP_PATH, SETUP),
      json(HttpMethod.Post, `/api/admin/servers/${hostServer.id}/ssh-identity`, generated),
    ],
  })

  await page.goto('/servers')
  await page.getByTestId(`server-ssh-${hostServer.id}`).click()
  await expect(page.getByTestId('server-ssh-install-command')).toHaveCount(0)
  await page.getByTestId('server-ssh-generate').click()

  const command = page.getByTestId('server-ssh-install-command')
  await expect(command).toContainText('sha256sum -c -')
  await expect(command).toContainText('e'.repeat(64))
  await expect(command).toContainText(`--binding '${generated.binding}'`)
  await expect(command).not.toContainText('PRIVATE')
})

test('enabling keeps the stored private key and stays in the dialog', async ({ page }) => {
  let stored: ServerSshSettingsDto = { ...hostServerSsh }
  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      json(HttpMethod.Get, '/api/admin/servers', [hostServer]),
      json(HttpMethod.Get, '/api/admin/server-groups', []),
      { method: HttpMethod.Get, path: SSH_PATH, respond: () => ({ body: stored }) },
      json(HttpMethod.Get, SETUP_PATH, SETUP),
      {
        method: HttpMethod.Put,
        path: SSH_PATH,
        respond: () => {
          stored = { ...stored, isEnabled: true }
          return { body: stored }
        },
      },
    ],
  })

  await page.goto('/servers')
  await page.getByTestId(`server-ssh-${hostServer.id}`).click()
  await expect(page.getByTestId('server-ssh-hostname')).toHaveValue(hostServerSsh.sshHostname)
  await expect(page.getByTestId('server-ssh-private-key')).toHaveValue('')

  await page.getByTestId('server-ssh-enabled').click()
  await page.getByTestId('server-ssh-save').click()

  await expect.poll(() => api.callsTo(HttpMethod.Put, SSH_PATH).length).toBe(1)
  const body = JSON.parse(api.callsTo(HttpMethod.Put, SSH_PATH)[0]?.postData ?? '{}') as Record<string, unknown>
  expect(body.isEnabled).toBe(true)
  expect(body.agentPrivateKey).toBeNull()
  await expect(page.getByTestId('server-ssh-hostname')).toBeVisible()
})

test('a test against an agent installed for another server explains the mismatch', async ({ page }) => {
  await stubApi(page, {
    signedIn: true,
    routes: [
      json(HttpMethod.Get, '/api/admin/servers', [hostServer]),
      json(HttpMethod.Get, '/api/admin/server-groups', []),
      json(HttpMethod.Get, SSH_PATH, { ...hostServerSsh, isHealthy: false }),
      json(HttpMethod.Get, SETUP_PATH, SETUP),
      json(HttpMethod.Post, `${SSH_PATH}/test`, { isHealthy: false, agentVersion: '1.1.0', error: 'binding_mismatch', provider: 'cwp' }),
    ],
  })

  await page.goto('/servers')
  await page.getByTestId(`server-ssh-${hostServer.id}`).click()
  await page.getByTestId('server-ssh-test').click()

  await expect(page.getByTestId('server-ssh-binding-mismatch')).toBeVisible()
  await expect(page.getByTestId('server-ssh-health')).toContainText('Agent unreachable')
})
