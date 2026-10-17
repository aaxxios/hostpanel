/**
 * Create-invoice flow: from a client's invoices tab the operator opens the inline form, fills
 * one line item, submits, and lands on the new invoice's detail page.
 */
import { expect, test } from '@playwright/test'
import { adaDetail } from './fixtures/clients'
import { createdInvoice, emptyClientInvoices, newInvoiceId } from './fixtures/billing'
import { fieldOf } from './support/locators'
import { json, stubApi } from './support/stubApi'

test('creating an invoice posts the line item and opens the new invoice', async ({ page }) => {
  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      json('GET', '/api/clients/7', adaDetail),
      json('GET', '/api/billing/client/7', emptyClientInvoices),
      json('POST', '/api/billing', { id: newInvoiceId }),
      // The detail page loads its invoice by id.
      json('GET', `/api/billing/${newInvoiceId}`, createdInvoice),
    ],
  })

  await page.goto('/clients/7/invoices')
  await page.getByTestId('invoice-create-toggle').click()

  const [item] = createdInvoice.items
  await fieldOf(page, 'invoice-item-description').fill(item?.description ?? '')
  await fieldOf(page, 'invoice-item-price').fill(String(item?.unitPrice))
  await fieldOf(page, 'invoice-item-price').press('Tab')
  await fieldOf(page, 'invoice-item-quantity').fill(String(item?.quantity))
  await fieldOf(page, 'invoice-item-quantity').press('Tab')
  await page.getByTestId('invoice-create-submit').click()

  await page.waitForURL(`**/billing/${newInvoiceId}`)
  await expect(page.getByTestId('invoice-detail-title')).toContainText(String(newInvoiceId))

  const [create] = api.callsTo('POST', '/api/billing')
  expect(create?.body).toMatchObject({
    clientId: 7,
    isDraft: true,
    items: [{ description: item?.description, unitPrice: item?.unitPrice, quantity: item?.quantity }],
  })
})
