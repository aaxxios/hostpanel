/**
 * Email verification: the staff link confirms the address through `POST /api/auth/confirm-email`
 * with the address and token from the query, and a refused token shows the API's sentence.
 *
 * The API is stubbed at the network layer (`support/stubApi.ts`).
 */
import { expect, test } from '@playwright/test'
import { json, stubApi } from './support/stubApi'

test('a valid link confirms the address and offers the way to sign in', async ({ page }) => {
  const api = await stubApi(page, {
    routes: [{ method: 'POST', path: '/api/auth/confirm-email', respond: () => ({ status: 200 }) }],
  })

  await page.goto('/verify-email?token=abc&email=admin%40example.test')

  await expect(page.getByTestId('verify-email-success')).toBeVisible()
  const [call] = api.callsTo('POST', '/api/auth/confirm-email')
  expect(call?.body).toEqual({ email: 'admin@example.test', token: 'abc' })
})

test('an expired token shows the sentence the API wrote', async ({ page }) => {
  await stubApi(page, {
    routes: [
      json('POST', '/api/auth/confirm-email', {
        type: 'TOKEN_INVALID_OR_EXPIRED',
        title: 'Bad Request',
        status: 400,
        detail: 'The link has expired.',
      }, 400),
    ],
  })

  await page.goto('/verify-email?token=old&email=admin%40example.test')

  await expect(page.getByTestId('verify-email-error')).toContainText('The link has expired.')
})
