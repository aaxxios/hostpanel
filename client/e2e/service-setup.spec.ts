/**
 * Flow: the first-time setup wizard of a hosting service.
 *
 * Domain, control-panel credentials, review. Each step refuses what the backend would refuse
 * (the wizard checks the typed values as they are typed); the review step sends the setup and,
 * once the backend accepts it, opens the service's management page. A refusal by the backend is
 * shown on the review step. A service that is already set up says so instead of offering the
 * wizard.
 *
 * The service has no domain, so the wizard asks for one; the stub does not store the typed domain
 * (see `routeManagement` in the stub), which keeps the management page that opens afterwards off
 * the network.
 */
import { expect, test } from './support/test'
import type { Page } from '@playwright/test'
import { SERVICE, SERVICE_PENDING, SETUP } from './support/fixtures.mjs'
import { resetStub, scriptStub, signIn, stubRequests, waitForHydration } from './support/helpers'

/**
 * Types the domain and the credentials and arrives on the review step.
 *
 * @param page The page showing the first step of the wizard.
 * @returns Resolves once the review step is on screen.
 */
const fillWizard = async (page: Page): Promise<void> => {
  await page.getByTestId('setup-domain').fill(SETUP.domain)
  await page.getByTestId('setup-domain-next').click()
  await page.getByTestId('setup-username').fill(SETUP.username)
  await page.getByTestId('setup-password').fill(SETUP.password)
  await page.getByTestId('setup-credentials-next').click()
  await expect(page.getByTestId('setup-step-review')).toBeVisible()
}

test.beforeEach(async ({ context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')
})

test('a hostname that is not valid cannot be continued with', async ({ page }) => {
  await page.goto(`/client/services/${SERVICE_PENDING.id}/setup`)
  await waitForHydration(page)

  await expect(page.getByTestId('setup-step-domain')).toBeVisible()
  await expect(page.getByTestId('setup-domain-next')).toBeDisabled()

  await page.getByTestId('setup-domain').fill('not a hostname')
  await expect(page.getByTestId('setup-domain-next')).toBeDisabled()

  await page.getByTestId('setup-domain').fill(SETUP.domain)
  await expect(page.getByTestId('setup-domain-next')).toBeEnabled()
})

test('the credentials step needs a username and a password of acceptable length', async ({ page }) => {
  await page.goto(`/client/services/${SERVICE_PENDING.id}/setup`)
  await waitForHydration(page)
  await page.getByTestId('setup-domain').fill(SETUP.domain)
  await page.getByTestId('setup-domain-next').click()

  await expect(page.getByTestId('setup-step-credentials')).toBeVisible()
  await expect(page.getByTestId('setup-credentials-next')).toBeDisabled()

  await page.getByTestId('setup-username').fill(SETUP.username)
  await page.getByTestId('setup-password').fill('short')
  await expect(page.getByTestId('setup-credentials-next')).toBeDisabled()

  await page.getByTestId('setup-password').fill(SETUP.password)
  await expect(page.getByTestId('setup-credentials-next')).toBeEnabled()

  // Back keeps what was typed.
  await page.getByTestId('setup-credentials-back').click()
  await expect(page.getByTestId('setup-domain')).toHaveValue(SETUP.domain)
})

test('finishing the wizard sends the setup and opens the service', async ({ page, request }) => {
  await page.goto(`/client/services/${SERVICE_PENDING.id}/setup`)
  await waitForHydration(page)
  await fillWizard(page)

  await page.getByTestId('setup-build').click()

  await expect(page).toHaveURL(new RegExp(`/client/services/${SERVICE_PENDING.id}$`))
  await expect(page.getByTestId('service-header')).toBeVisible()
  await expect(page.getByTestId('service-setup-required')).toHaveCount(0)

  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === `/api/me/services/${SERVICE_PENDING.id}/setup`)
  expect(sent?.body).toMatchObject({ domain: SETUP.domain, username: SETUP.username, password: SETUP.password })
})

test('a setup the backend refuses shows its sentence and stays on the review step', async ({ page, request }) => {
  await scriptStub(request, { method: 'POST', path: `/api/me/services/${SERVICE_PENDING.id}/setup`, status: 400, code: 'SETUP_REFUSED', detail: 'E2E: the account could not be created.' })

  await page.goto(`/client/services/${SERVICE_PENDING.id}/setup`)
  await waitForHydration(page)
  await fillWizard(page)
  await page.getByTestId('setup-build').click()

  await expect(page.getByTestId('setup-error')).toContainText('E2E: the account could not be created.')
  await expect(page.getByTestId('setup-step-review')).toBeVisible()
  await expect(page).toHaveURL(/\/setup$/)
})

test('a service that is already set up says so instead of offering the wizard', async ({ page }) => {
  await page.goto(`/client/services/${SERVICE.id}/setup`)
  await waitForHydration(page)

  await expect(page.getByTestId('setup-go-manage')).toHaveAttribute('href', `/client/services/${SERVICE.id}`)
  await expect(page.getByTestId('setup-wizard')).toHaveCount(0)
})
