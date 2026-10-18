/**
 * Flow: transferring a domain in.
 *
 * The public transfer page hands the typed name and authorisation code on to the client-area form;
 * the form orders the transfer with the saved payment method and leads to the invoice the order
 * raised. A refused order keeps the form with the backend's sentence.
 */
import { expect, test } from './support/test'
import { DOMAIN_ORDER, TRANSFER } from './support/fixtures.mjs'
import { resetStub, scriptStub, signIn, stubRequests, waitForHydration } from './support/helpers'

test.beforeEach(async ({ context, request, baseURL }) => {
  await resetStub(request)
  await signIn(context, baseURL ?? 'http://127.0.0.1:3100')
})

test('the public transfer page carries the typed values to the client-area form', async ({ page }) => {
  await page.goto('/domains/transfer')
  await waitForHydration(page)

  await page.getByTestId('domains-transfer-domain').fill(TRANSFER.domain)
  await page.getByTestId('domains-transfer-epp').fill(TRANSFER.eppCode)
  await page.getByTestId('domains-transfer-submit').click()

  await expect(page).toHaveURL(/\/client\/domains\/transfer\?domain=/)
  await expect(page.getByTestId('transfer-domain')).toHaveValue(TRANSFER.domain)
  await expect(page.getByTestId('transfer-epp')).toHaveValue(TRANSFER.eppCode)
})

test('the transfer is ordered and leads to its invoice', async ({ page, request }) => {
  await page.goto('/client/domains/transfer')
  await waitForHydration(page)

  // Nothing can be ordered without a domain.
  await expect(page.getByTestId('transfer-submit')).toBeDisabled()

  await page.getByTestId('transfer-domain').fill(TRANSFER.domain)
  await page.getByTestId('transfer-epp').fill(TRANSFER.eppCode)
  await expect(page.getByTestId('transfer-submit')).toBeEnabled()
  await page.getByTestId('transfer-submit').click()

  await expect(page.getByTestId('transfer-success')).toBeVisible()
  await expect(page.getByTestId('transfer-pay-invoice')).toHaveAttribute('href', `/client/invoices/${DOMAIN_ORDER.invoiceId}`)

  const order = (await stubRequests(request)).find(r => r.method === 'POST' && r.path === '/api/me/domains/transfer-order')
  expect(order?.body).toMatchObject({ domainName: TRANSFER.domain, eppCode: TRANSFER.eppCode, years: 1, paymentMethod: 'stripe' })
})

test('a refused transfer keeps the form and shows the backend sentence', async ({ page, request }) => {
  await scriptStub(request, { method: 'POST', path: '/api/me/domains/transfer-order', status: 400, code: 'TRANSFER_REFUSED', detail: 'E2E: this domain cannot be transferred.' })

  await page.goto('/client/domains/transfer')
  await waitForHydration(page)
  await page.getByTestId('transfer-domain').fill(TRANSFER.domain)
  await page.getByTestId('transfer-submit').click()

  await expect(page.getByTestId('ui-form-error')).toContainText('E2E: this domain cannot be transferred.')
  await expect(page.getByTestId('transfer-success')).toHaveCount(0)
  await expect(page.getByTestId('domain-transfer-form')).toBeVisible()
})

test('the back link leads to the domains list', async ({ page }) => {
  await page.goto('/client/domains/transfer')
  await waitForHydration(page)

  await page.getByTestId('transfer-back').click()

  await expect(page).toHaveURL(/\/client\/domains$/)
})
