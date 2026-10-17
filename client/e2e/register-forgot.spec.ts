/**
 * Flow: registration with a field refusal, and password recovery.
 *
 * Registration shows the sentence the backend put on the refused field (the page writes none);
 * the recovery form confirms that the request went out and offers the way back to sign in.
 */
import { expect, test } from './support/test'
import { CLIENT, TAKEN_EMAIL, TAKEN_EMAIL_MESSAGE } from './support/fixtures.mjs'
import { resetStub, stubRequests, waitForHydration } from './support/helpers'

test('a refused registration marks the email field with the backend sentence', async ({ page, request }) => {
  await resetStub(request)
  await page.goto('/client/register')
  await waitForHydration(page)

  await page.getByTestId('register-firstname').fill('Eve')
  await page.getByTestId('register-lastname').fill('Tester')
  await page.getByTestId('register-email').fill(TAKEN_EMAIL)
  await page.getByTestId('register-password').fill('a-long-enough-password-1')
  await page.getByTestId('register-submit').click()

  await expect(page.getByTestId('register-email')).toHaveAttribute('aria-invalid', 'true')
  await expect(page.getByTestId('register-form')).toContainText(TAKEN_EMAIL_MESSAGE)
  await expect(page).toHaveURL(/\/client\/register$/)
})

test('a password recovery request reaches the backend and confirms to the visitor', async ({ page, request }) => {
  await resetStub(request)
  await page.goto('/client/forgot')
  await waitForHydration(page)

  await page.getByTestId('forgot-email').fill(CLIENT.email)
  await page.getByTestId('forgot-submit').click()

  await expect(page.getByTestId('forgot-sent-back')).toBeVisible()
  await expect(page.getByTestId('forgot-form')).toHaveCount(0)

  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/auth/forgot-password')
  expect(sent?.body).toMatchObject({ email: CLIENT.email })
})
