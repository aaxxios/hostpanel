/**
 * The SSH Keys panel of a hosting service: connection details, adding a key, the API's refusals,
 * the browser-side private-key guard, revoking with confirmation, and the mobile layout.
 */
import { expect, test } from './support/test'
import { SERVICE_MANAGED, SSH_ACCESS, SSH_PUBLIC_KEY } from './support/fixtures.mjs'
import { resetStub, scriptStub, signIn, stubRequests, waitForHydration } from './support/helpers'

/** The SSH keys collection of the managed service, as the backend path reads. */
const KEYS_PATH = `/api/me/services/${SERVICE_MANAGED.id}/ssh/keys`

test.beforeEach(async ({ context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')
})

/**
 * Opens the managed service on its SSH Keys tab.
 *
 * @param page - The page.
 * @returns Resolves once the panel shows the connect command.
 */
const openSshTab = async (page: import('@playwright/test').Page): Promise<void> => {
  await page.goto(`/client/services/${SERVICE_MANAGED.id}`)
  await waitForHydration(page)
  await page.getByTestId('service-tab-ssh').click()
  await expect(page.getByTestId('service-ssh-command')).toBeVisible()
}

test('the panel shows where to connect, resolved for this service', async ({ page }) => {
  await openSshTab(page)

  await expect(page.getByTestId('service-ssh-command')).toHaveText(`ssh -p ${SSH_ACCESS.port} ${SERVICE_MANAGED.username}@${SSH_ACCESS.hostname}`)
  await expect(page.getByTestId('service-ssh-empty')).toBeVisible()
})

test('a key is added, listed, and revoked only after confirmation', async ({ page, request }) => {
  await openSshTab(page)

  await page.getByTestId('service-ssh-public-key').fill(SSH_PUBLIC_KEY)
  await expect(page.getByTestId('service-ssh-label')).toHaveValue('e2e@laptop')
  await page.getByTestId('service-ssh-label').fill('E2E laptop')
  await page.getByTestId('service-ssh-add').click()
  await expect(page.getByTestId('service-ssh-key-label')).toHaveText('E2E laptop')

  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === KEYS_PATH)
  expect(sent?.body).toEqual({ label: 'E2E laptop', publicKey: SSH_PUBLIC_KEY })

  await page.getByTestId('service-ssh-revoke').click()
  expect((await stubRequests(request)).some(r => r.method === 'DELETE')).toBe(false)
  await page.getByTestId('ui-confirm-confirm').click()
  await expect(page.getByTestId('service-ssh-key')).toHaveCount(0)
  await expect(page.getByTestId('service-ssh-empty')).toBeVisible()
})

test('a private key is refused in the browser and never sent', async ({ page, request }) => {
  await openSshTab(page)

  await page.getByTestId('service-ssh-public-key').fill('-----BEGIN OPENSSH PRIVATE KEY-----\nNOT-A-REAL-KEY\n-----END OPENSSH PRIVATE KEY-----')
  await page.getByTestId('service-ssh-label').fill('oops')
  await page.getByTestId('service-ssh-add').click()

  await expect(page.getByTestId('ui-form-error')).toBeVisible()
  expect((await stubRequests(request)).some(r => r.method === 'POST' && r.path === KEYS_PATH)).toBe(false)
})

test('the API refusal is shown in the reader\'s language, as the API wrote it', async ({ page, request }) => {
  await scriptStub(request, { method: 'POST', path: KEYS_PATH, status: 503, code: 'SSH_SERVER_UNAVAILABLE', detail: 'The hosting server could not be reached. Please try again later.' })
  await openSshTab(page)

  await page.getByTestId('service-ssh-public-key').fill(SSH_PUBLIC_KEY)
  await page.getByTestId('service-ssh-add').click()

  await expect(page.getByTestId('ui-form-error')).toContainText('could not be reached')
})

test('the panel fits a phone screen without sideways scrolling', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  await openSshTab(page)

  const overflow = await page.evaluate(() => document.documentElement.scrollWidth - window.innerWidth)
  expect(overflow).toBeLessThanOrEqual(1)
})
