/**
 * Flow: the unconfirmed-email banner of the client area.
 *
 * While the backend reports the signed-in address as unconfirmed the client area shows a banner
 * with a resend button; pressing it posts to the backend and confirms to the person. A confirmed
 * address shows no banner, and the confirmation link lands on the existing confirmation page.
 */
import { expect, test } from './support/test'
import { CLIENT } from './support/fixtures.mjs'
import { resetStub, scriptStub, setEmailVerified, signIn, stubRequests, waitForHydration } from './support/helpers'
import { HttpMethod } from '../types/httpmethod'

test('an unconfirmed address shows the banner and the resend button reaches the backend', async ({ page, context, request, baseURL }) => {
  await resetStub(request)
  await setEmailVerified(request, false)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')

  await page.goto('/client/dashboard')
  await waitForHydration(page)
  await expect(page.getByTestId('verify-email-banner')).toBeVisible()

  await page.getByTestId('verify-email-resend').click()

  await expect(page.getByTestId('verify-email-message')).not.toBeEmpty()
  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/auth/resend-verification')
  expect(sent).toBeDefined()
})

test('a refused resend shows the backend sentence', async ({ page, context, request, baseURL }) => {
  await resetStub(request)
  await setEmailVerified(request, false)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')
  await scriptStub(request, { method: HttpMethod.Post, path: '/api/auth/resend-verification', status: 400, code: 'RATE_LIMITED', detail: 'E2E: wait before asking again.' })

  await page.goto('/client/dashboard')
  await waitForHydration(page)
  await page.getByTestId('verify-email-resend').click()

  await expect(page.getByTestId('verify-email-message')).toContainText('E2E: wait before asking again.')
})

test('a confirmed address shows no banner', async ({ page, context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')

  await page.goto('/client/dashboard')
  await waitForHydration(page)

  await expect(page.getByTestId('verify-email-banner')).toHaveCount(0)
})

test('the confirmation link reaches the backend with the address and token', async ({ page, request }) => {
  await resetStub(request)

  await page.goto(`/client/confirm-email?token=tok-e2e&email=${encodeURIComponent(CLIENT.email)}`)
  await waitForHydration(page)
  // The page posts on mount; the result line is the signal that the request has been answered.
  await expect(page.getByTestId('confirm-success')).toBeVisible()

  const sent = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/auth/confirm-email')
  expect(sent?.body).toMatchObject({ email: CLIENT.email, token: 'tok-e2e' })
})
