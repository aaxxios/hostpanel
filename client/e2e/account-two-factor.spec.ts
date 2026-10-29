/**
 * Flow: two-factor authentication on the account.
 *
 * Enabling shows the secret and its QR code, then asks for a code from the authenticator; once the
 * backend accepts it the card offers to disable instead, and disabling asks for the current
 * password and a code, then signs the browser out. A refused code keeps the dialog open with the
 * backend's sentence.
 */
import { expect, test } from './support/test'
import { TWO_FACTOR, TWO_FACTOR_REFUSED } from './support/fixtures.mjs'
import { enterOtp } from './support/forms'
import { resetStub, scriptStub, signIn, stubRequests, waitForHydration } from './support/helpers'
import { HttpMethod } from '../types/httpmethod'

test.beforeEach(async ({ page, context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')

  await page.goto('/client/account')
  await waitForHydration(page)
  await expect(page.getByTestId('account-2fa-card')).toBeVisible()
})

test('enrolment shows the secret and the QR code, and an accepted code turns two-factor on', async ({ page, request }) => {
  await expect(page.getByTestId('account-2fa-disable')).toHaveCount(0)
  await page.getByTestId('account-2fa-enable').click()

  await expect(page.getByTestId('account-2fa-dialog')).toBeVisible()
  await expect(page.getByTestId('account-2fa-secret')).toContainText(TWO_FACTOR.secret)
  await expect(page.getByTestId('account-2fa-qr')).toBeVisible()

  await enterOtp(page, TWO_FACTOR.code)
  await page.getByTestId('account-2fa-submit').click()

  await expect(page.getByTestId('account-2fa-dialog')).toHaveCount(0)
  await expect(page.getByTestId('account-2fa-disable')).toBeVisible()
  await expect(page.getByTestId('account-2fa-enable')).toHaveCount(0)

  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/me/2fa/enable')
  expect(sent?.body).toMatchObject({ code: TWO_FACTOR.code })
})

test('a refused code keeps the dialog open with the backend sentence', async ({ page, request }) => {
  await scriptStub(request, { method: HttpMethod.Post, path: '/api/me/2fa/enable', status: 400, code: 'INVALID_CODE', detail: TWO_FACTOR_REFUSED })

  await page.getByTestId('account-2fa-enable').click()
  await enterOtp(page, TWO_FACTOR.code)
  await page.getByTestId('account-2fa-submit').click()

  await expect(page.getByTestId('ui-form-error')).toContainText(TWO_FACTOR_REFUSED)
  await expect(page.getByTestId('account-2fa-dialog')).toBeVisible()
  await expect(page.getByTestId('account-2fa-disable')).toHaveCount(0)
})

test('with two-factor on, the password and a code turn it off and sign this browser out', async ({ page, request }) => {
  await page.getByTestId('account-2fa-enable').click()
  await enterOtp(page, TWO_FACTOR.code)
  await page.getByTestId('account-2fa-submit').click()
  await expect(page.getByTestId('account-2fa-disable')).toBeVisible()

  await page.getByTestId('account-2fa-disable').click()
  await expect(page.getByTestId('account-2fa-dialog')).toBeVisible()
  // Disabling needs no enrolment: there is no secret or QR code on screen.
  await expect(page.getByTestId('account-2fa-secret')).toHaveCount(0)
  await page.getByTestId('account-2fa-password').fill('current-password')
  await enterOtp(page, TWO_FACTOR.code)
  await page.getByTestId('account-2fa-submit').click()

  // Switching it off ends every session of the account, so the portal leaves for the sign-in page.
  await expect(page).toHaveURL(/\/client\/login/)
  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/me/2fa/disable')
  expect(sent?.body).toMatchObject({ code: TWO_FACTOR.code, currentPassword: 'current-password' })
})
