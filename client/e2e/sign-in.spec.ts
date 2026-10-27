/**
 * Flow: sign-in (local auth mode).
 *
 * A customer signs in with email and password and lands on the dashboard; a wrong password is
 * refused with a message and leaves them on the form. The session the sign-in leaves is one small
 * opaque cookie: no token reaches the browser, and no cookie comes near the 4 KB browser limit.
 */
import { expect, test } from './support/test'
import { ACCESS_TOKEN, CLIENT_CREDENTIALS, WRONG_PASSWORD } from './support/fixtures.mjs'
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

test('the session is one small opaque cookie, with no token in the browser', async ({ page, context }) => {
  await page.goto('/client/login')
  await waitForHydration(page)

  await page.getByTestId('login-email').fill(CLIENT_CREDENTIALS.email)
  await page.getByTestId('login-password').fill(CLIENT_CREDENTIALS.password)
  await page.getByTestId('login-submit').click()
  await expect(page.getByTestId('dashboard-title')).toBeVisible()

  const cookies = await context.cookies()
  const session = cookies.find((cookie) => cookie.name === 'portal_session')
  expect(session?.httpOnly).toBe(true)
  expect(session?.value).not.toContain(ACCESS_TOKEN)
  expect(cookies.map((cookie) => cookie.name)).not.toContain('auth_token')
  for (const cookie of cookies) {
    expect(`${cookie.name}=${cookie.value}`.length).toBeLessThan(200)
  }
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
