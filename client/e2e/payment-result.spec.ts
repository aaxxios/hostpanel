/**
 * Flow: the hosted-gateway return page.
 *
 * After paying at the bank, the payer lands here with the invoice in the address; the page asks
 * the backend what happened and says so. Each outcome has its own screen and its own next step,
 * and a question the backend cannot answer is never reported as a payment result.
 */
import { expect, test } from './support/test'
import { GATEWAY_DECLINED_INVOICE, GATEWAY_PAID_INVOICE, GATEWAY_PENDING_INVOICE } from './support/fixtures.mjs'
import { resetStub, signIn, stubRequests, waitForHydration } from './support/helpers'

test.beforeEach(async ({ context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')
})

test('a paid invoice offers the way on to the invoice', async ({ page, request }) => {
  await page.goto(`/payment/result?invoice=${GATEWAY_PAID_INVOICE}`)
  await waitForHydration(page)

  await expect(page.getByTestId('payment-continue')).toBeVisible()
  await page.getByTestId('payment-continue').click()
  await expect(page).toHaveURL(new RegExp(`/client/invoices/${GATEWAY_PAID_INVOICE}$`))

  const asked = (await stubRequests(request)).some(r => r.method === 'POST' && r.path === `/api/me/invoices/${GATEWAY_PAID_INVOICE}/gateway-payment/complete`)
  expect(asked).toBe(true)
})

test('a payment still being confirmed can be checked again', async ({ page }) => {
  await page.goto(`/payment/result?invoice=${GATEWAY_PENDING_INVOICE}`)
  await waitForHydration(page)

  await expect(page.getByTestId('payment-retry')).toBeVisible()
  await expect(page.getByTestId('payment-continue')).toHaveCount(0)
})

test('a declined payment leads back to the pay page', async ({ page }) => {
  await page.goto(`/payment/result?invoice=${GATEWAY_DECLINED_INVOICE}`)
  await waitForHydration(page)

  await page.getByTestId('payment-try-again').click()
  await expect(page).toHaveURL(new RegExp(`/client/invoices/${GATEWAY_DECLINED_INVOICE}/pay$`))
})

test('an address without an invoice or order is reported as missing, not as a payment result', async ({ page }) => {
  await page.goto('/payment/result')
  await waitForHydration(page)

  await expect(page.getByTestId('payment-invoices')).toBeVisible()
  await expect(page.getByTestId('payment-continue')).toHaveCount(0)
  await expect(page.getByTestId('payment-retry')).toHaveCount(0)
})

test('an answer the backend cannot give is unknown, with a retry and a status link', async ({ page }) => {
  await page.goto('/payment/result?invoice=599')
  await waitForHydration(page)

  await expect(page.getByTestId('payment-retry')).toBeVisible()
  await expect(page.getByTestId('payment-status')).toBeVisible()
  await expect(page.getByTestId('payment-continue')).toHaveCount(0)
})
