/**
 * Flow: demo mode.
 *
 * While the backend reports a demo, every page carries the demo banner (label, time to the next
 * reset, the data-resets note), and the sign-in page offers the published customer account with a
 * button that fills the form. The admin account is never offered here.
 */
import { expect, test } from './support/test'
import { DEMO_ACCOUNTS } from './support/fixtures.mjs'
import { enableDemoMode, resetStub, waitForHydration } from './support/helpers'

/**
 * Finds one published account by role, failing loudly when the fixture lacks it.
 *
 * @param role - The area the account signs in to.
 * @returns The account.
 */
const accountFor = (role: string) => {
  const account = DEMO_ACCOUNTS.find((candidate) => candidate.role === role)
  if (!account) {
    throw new Error(`The demo fixture has no ${role} account.`)
  }
  return account
}

/** The published customer account. */
const CLIENT_ACCOUNT = accountFor('Client')

/** The published admin account, which the storefront must never offer. */
const ADMIN_ACCOUNT = accountFor('Admin')

test.beforeEach(async ({ request }) => {
  await resetStub(request)
  await enableDemoMode(request)
})

test.afterEach(async ({ request }) => {
  await resetStub(request)
})

test('the demo banner shows the label, the time to the reset and the note', async ({ page }) => {
  await page.goto('/client/login')
  await waitForHydration(page)

  await expect(page.getByTestId('demo-banner')).toBeVisible()
  await expect(page.getByTestId('demo-banner-label')).toBeVisible()
  await expect(page.getByTestId('demo-banner-reset')).toContainText(/2\s*h|2\s*ч|2\s*ժ/)
  await expect(page.getByTestId('demo-banner-note')).toBeVisible()
})

test('the sign-in page offers the client demo account and fills the form from it', async ({ page }) => {
  await page.goto('/client/login')
  await waitForHydration(page)

  await expect(page.getByTestId('login-demo-hint')).toContainText(CLIENT_ACCOUNT.email)
  await expect(page.getByTestId('login-demo')).not.toContainText(ADMIN_ACCOUNT.email)

  await page.getByTestId('login-demo-use').click()

  await expect(page.getByTestId('login-email')).toHaveValue(CLIENT_ACCOUNT.email)
  await expect(page.getByTestId('login-password')).toHaveValue(CLIENT_ACCOUNT.password)
})
