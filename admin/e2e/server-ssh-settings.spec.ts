/**
 * Server SSH key management settings: opening the dialog from a server row, enabling with the
 * stored key kept (never read back), and the agent health check.
 */
import { HttpMethod } from '../src/types/http'
import { expect, test } from '@playwright/test'
import { hostServer, hostServerSsh } from './fixtures/servers'
import type { ServerSshSettingsDto } from '../src/types/serverssh'
import { json, stubApi } from './support/stubApi'

/** The settings path of the fixture server. */
const SSH_PATH = `/api/admin/servers/${hostServer.id}/ssh-settings`

test('enabling SSH keys keeps the stored private key and closes the dialog', async ({ page }) => {
  let stored: ServerSshSettingsDto = { ...hostServerSsh }
  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      json(HttpMethod.Get, '/api/admin/servers', [hostServer]),
      json(HttpMethod.Get, '/api/admin/server-groups', []),
      { method: HttpMethod.Get, path: SSH_PATH, respond: () => ({ body: stored }) },
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

  await expect(page.getByTestId('server-ssh-hostname')).toHaveCount(0)
  const [put] = api.callsTo(HttpMethod.Put, SSH_PATH)
  const body = JSON.parse(put?.postData ?? '{}') as Record<string, unknown>
  expect(body.isEnabled).toBe(true)
  expect(body.agentPrivateKey).toBeNull()
})

test('the agent health check shows the outcome in the dialog', async ({ page }) => {
  await stubApi(page, {
    signedIn: true,
    routes: [
      json(HttpMethod.Get, '/api/admin/servers', [hostServer]),
      json(HttpMethod.Get, '/api/admin/server-groups', []),
      json(HttpMethod.Get, SSH_PATH, { ...hostServerSsh, isHealthy: false }),
      json(HttpMethod.Post, `${SSH_PATH}/test`, { isHealthy: false, agentVersion: null, error: 'unreachable' }),
    ],
  })

  await page.goto('/servers')
  await page.getByTestId(`server-ssh-${hostServer.id}`).click()
  await page.getByTestId('server-ssh-test').click()

  await expect(page.getByTestId('server-ssh-agent-error')).toContainText('unreachable')
  await expect(page.getByTestId('server-ssh-health')).toContainText('Agent unreachable')
})
