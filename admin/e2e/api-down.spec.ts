/**
 * API outage: when a request gets no answer at all, a screen shows the one shared "could not reach
 * the server" sentence from the catalogue, never the browser's raw network error.
 */
import { readFileSync } from 'node:fs'
import { join } from 'node:path'
import { expect, test } from '@playwright/test'
import { stubApi } from './support/stubApi'

/** The shared sentence for "no answer", read from the English catalogue so the spec follows its wording. */
const noAnswer = (JSON.parse(readFileSync(join(process.cwd(), 'src/locales/en/errors.json'), 'utf8')) as { noAnswer: string }).noAnswer

test('a request that gets no answer shows the shared "could not reach the server" sentence', async ({ page }) => {
  await stubApi(page, { signedIn: true })
  // Registered after the stub, so it runs first: the clients endpoint drops the connection.
  await page.route((url) => url.pathname === '/api/clients', (route) => route.abort('connectionrefused'))

  await page.goto('/clients')

  await expect(page.getByTestId('clients-list-error')).toContainText(noAnswer)
  await expect(page.getByTestId('clients-list-error')).not.toContainText('Failed to fetch')
})

test('the same sentence appears on another screen', async ({ page }) => {
  await stubApi(page, { signedIn: true })
  await page.route((url) => url.pathname === '/api/orders', (route) => route.abort('connectionrefused'))

  await page.goto('/orders')

  await expect(page.getByTestId('orders-error')).toContainText(noAnswer)
})
