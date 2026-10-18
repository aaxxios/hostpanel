/**
 * Sign-in flow: a signed-out visitor is sent to the login page, a rejected password shows the
 * API's own sentence and keeps them there, and correct credentials land on the dashboard.
 *
 * The API is stubbed at the network layer (`support/stubApi.ts`); `/api/auth/me` answers 401
 * until the stubbed login call has accepted the credentials, as the real one does.
 */
import { expect, test } from '@playwright/test'
import { loginRejected, validCredentials } from './fixtures/auth'
import { fieldOf } from './support/locators'
import { stubApi } from './support/stubApi'

test('a signed-out visitor opening a protected page is sent to the login form', async ({ page }) => {
  await stubApi(page)

  await page.goto('/dashboard')

  await page.waitForURL('**/login')
  await expect(fieldOf(page, 'login-email')).toBeVisible()
  await expect(fieldOf(page, 'login-password')).toBeVisible()
})

test('rejected credentials show the sentence the API returned and stay on the login page', async ({ page }) => {
  const api = await stubApi(page)

  await page.goto('/login')
  await fieldOf(page, 'login-email').fill(validCredentials.email)
  await fieldOf(page, 'login-password').fill('wrong-password')
  await page.getByTestId('login-submit').click()

  await expect(page.getByTestId('login-error')).toContainText(loginRejected.detail)
  expect(new URL(page.url()).pathname).toBe('/login')
  expect(api.callsTo('POST', '/api/auth/login')).toHaveLength(1)
})

test('correct credentials sign in and open the dashboard', async ({ page }) => {
  const api = await stubApi(page)

  await page.goto('/login')
  await fieldOf(page, 'login-email').fill(validCredentials.email)
  await fieldOf(page, 'login-password').fill(validCredentials.password)
  await page.getByTestId('login-submit').click()

  await page.waitForURL('**/dashboard')
  await expect(page.getByTestId('dashboard-header')).toBeVisible()

  // The credentials travel in the JSON body — never in the URL.
  const [login] = api.callsTo('POST', '/api/auth/login')
  expect(login?.body).toEqual({ email: validCredentials.email, password: validCredentials.password })
  expect(login?.search).toBe('')
})
