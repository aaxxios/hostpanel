/**
 * Flow: sign-in for an account with two-factor on (local auth mode).
 *
 * The password step does not open a session; the page swaps to the code step, and only an accepted
 * code lands the customer on the dashboard. A refused code keeps the step open with the backend's
 * sentence, and the way back returns to the password form.
 */
import { expect, test } from './support/test'
import {
  TWO_FACTOR_CREDENTIALS,
  TWO_FACTOR_LOGIN_CODE,
  TWO_FACTOR_PENDING_TOKEN,
  TWO_FACTOR_REFUSED
} from './support/fixtures.mjs'
import { enterOtp } from './support/forms'
import { resetStub, stubRequests, waitForHydration } from './support/helpers'

test.beforeEach(async ({ page, request }) => {
  await resetStub(request)
  await page.goto('/client/login')
  await waitForHydration(page)

  await page.getByTestId('login-email').fill(TWO_FACTOR_CREDENTIALS.email)
  await page.getByTestId('login-password').fill(TWO_FACTOR_CREDENTIALS.password)
  await page.getByTestId('login-submit').click()
  await expect(page.getByTestId('login-2fa-form')).toBeVisible()
})

test('the password step opens the code step instead of the dashboard', async ({ page }) => {
  await expect(page.getByTestId('login-form')).toHaveCount(0)
  await expect(page).toHaveURL(/\/client\/login/)
})

test('an accepted code finishes the sign-in and opens the dashboard', async ({ page, request }) => {
  // The sixth digit completes the code and submits it; no button press is needed.
  await enterOtp(page, TWO_FACTOR_LOGIN_CODE)

  await expect(page).toHaveURL(/\/client\/dashboard$/)
  await expect(page.getByTestId('dashboard-title')).toBeVisible()

  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/auth/2fa-login')
  expect(sent?.body).toMatchObject({ pendingToken: TWO_FACTOR_PENDING_TOKEN, code: TWO_FACTOR_LOGIN_CODE })
})

test('a refused code keeps the step open with the backend sentence', async ({ page }) => {
  await enterOtp(page, '000000')

  await expect(page.getByTestId('login-error')).toContainText(TWO_FACTOR_REFUSED)
  await expect(page.getByTestId('login-2fa-form')).toBeVisible()
  await expect(page).toHaveURL(/\/client\/login/)
})

test('going back returns to the password form', async ({ page }) => {
  await page.getByTestId('login-2fa-cancel').click()

  await expect(page.getByTestId('login-form')).toBeVisible()
  await expect(page.getByTestId('login-2fa-form')).toHaveCount(0)
})
