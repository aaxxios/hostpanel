/**
 * Flow: sign-in (local auth mode).
 *
 * A customer signs in with email and password and lands on the dashboard; a wrong password is
 * refused with a message and leaves them on the form.
 */
import { expect, test } from './support/test'
import { CLIENT_CREDENTIALS, WRONG_PASSWORD } from './support/fixtures.mjs'
import { waitForHydration } from './support/helpers'

test('valid credentials open the dashboard', async ({ page }) => {
  await page.goto('/client/login')
  await waitForHydration(page)

  await page.getByTestId('login-email').fill(CLIENT_CREDENTIALS.email)
  await page.getByTestId('login-password').fill(CLIENT_CREDENTIALS.password)
  await page.getByTestId('login-submit').click()

  await expect(page).toHaveURL(/\/client\/dashboard$/)
  await expect(page.getByTestId('dashboard-title')).toBeVisible()
})

test('a wrong password is refused and the form stays', async ({ page }) => {
  await page.goto('/client/login')
  await waitForHydration(page)

  await page.getByTestId('login-email').fill(CLIENT_CREDENTIALS.email)
  await page.getByTestId('login-password').fill(WRONG_PASSWORD)
  await page.getByTestId('login-submit').click()

  await expect(page.getByTestId('login-error')).toBeVisible()
  await expect(page).toHaveURL(/\/client\/login/)
})
