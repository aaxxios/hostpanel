/**
 * Flow: the sign-in gate and the return trip.
 *
 * A signed-out visitor who opens a client-area address is sent to the sign-in form carrying that
 * address; signing in brings them back to it rather than to the dashboard. An invitation link does
 * the same: it explains that a session is needed, then returns to the very link, token and all.
 */
import { expect, test } from './support/test'
import type { Page } from '@playwright/test'
import { CLIENT_CREDENTIALS, INVOICE } from './support/fixtures.mjs'
import { resetStub, stubRequests, waitForHydration } from './support/helpers'

/**
 * Signs in through the form that is on screen.
 *
 * @param page The page showing the sign-in form.
 * @returns Resolves once the credentials are submitted.
 */
const signInThroughForm = async (page: Page): Promise<void> => {
  await waitForHydration(page)
  await page.getByTestId('login-email').fill(CLIENT_CREDENTIALS.email)
  await page.getByTestId('login-password').fill(CLIENT_CREDENTIALS.password)
  await page.getByTestId('login-submit').click()
}

test('a signed-out visitor is sent to sign in and returned to the page they asked for', async ({ page }) => {
  await page.goto(`/client/invoices/${INVOICE.id}`)

  await expect(page).toHaveURL(/\/client\/login\?redirect=/)
  await signInThroughForm(page)

  await expect(page).toHaveURL(new RegExp(`/client/invoices/${INVOICE.id}$`))
  await expect(page.getByTestId('invoice-heading')).toBeVisible()
})

test('an invitation link without a token says it is invalid', async ({ page }) => {
  await page.goto('/client/accept-invite')
  await waitForHydration(page)

  await expect(page.getByTestId('invite-invalid')).toBeVisible()
  await expect(page.getByTestId('invite-form')).toHaveCount(0)
})

test('accepting an invitation needs a session, then returns to the link and accepts', async ({ page, request }) => {
  await resetStub(request)
  await page.goto('/client/accept-invite?token=invite-e2e')
  await waitForHydration(page)

  // Signed out: the BFF refuses before any backend call and the page offers the sign-in step.
  await page.getByTestId('invite-submit').click()
  await expect(page.getByTestId('invite-sign-in-required')).toBeVisible()
  expect((await stubRequests(request)).some(r => r.path === '/api/auth/accept-invite')).toBe(false)

  await page.getByTestId('invite-sign-in').click()
  await expect(page).toHaveURL(/\/client\/login\?redirect=/)
  await signInThroughForm(page)

  // Back on the invitation, now signed in: accepting reaches the backend with the token.
  await expect(page).toHaveURL(/\/client\/accept-invite\?token=invite-e2e$/)
  await waitForHydration(page)
  await page.getByTestId('invite-submit').click()

  await expect(page).toHaveURL(/\/client\/dashboard$/)
  const accepted = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/auth/accept-invite')
  expect(accepted?.body).toMatchObject({ token: 'invite-e2e' })
})
