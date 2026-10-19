/**
 * Demo mode: the login screen's "Use demo account" button fills in the published Admin
 * credentials, and the banner with the reset note appears once signed in.
 *
 * The API is stubbed at the network layer (`support/stubApi.ts`).
 */
import { HttpMethod } from '../src/types/http'
import { expect, test } from '@playwright/test'
import { fieldOf } from './support/locators'
import { json, stubApi } from './support/stubApi'

/** `GET /api/public/demo` answer for a running demo. */
const demoOn = {
  enabled: true,
  nextResetAt: new Date(Date.now() + 90 * 60_000).toISOString(),
  accounts: [{ role: 'Admin', email: 'demo-admin@example.test', password: 'demo-pass' }],
}

test('the login screen prefills the published demo Admin account', async ({ page }) => {
  await stubApi(page, { routes: [json(HttpMethod.Get, '/api/public/demo', demoOn)] })

  await page.goto('/login')
  await page.getByTestId('demo-login-use').click()

  await expect(fieldOf(page, 'login-email')).toHaveValue('demo-admin@example.test')
  await expect(fieldOf(page, 'login-password')).toHaveValue('demo-pass')
})

test('the banner shows the label and the countdown once signed in', async ({ page }) => {
  await stubApi(page, { signedIn: true, routes: [json(HttpMethod.Get, '/api/public/demo', demoOn)] })

  await page.goto('/dashboard')

  await expect(page.getByTestId('demo-banner')).toContainText('Demo mode')
  await expect(page.getByTestId('demo-banner-countdown')).toBeVisible()
})
