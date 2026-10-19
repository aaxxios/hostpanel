/**
 * Invoices list flow: ticking invoices reveals the bulk bar, "mark paid" posts the ticked ids
 * and reloads the list, and a refused load shows the API's sentence.
 */
import { HttpMethod } from '../src/types/http'
import { expect, test } from '@playwright/test'
import { invoicesPage, paidInvoice, unpaidInvoice } from './fixtures/invoices'
import type { Invoice } from '../src/types/invoice'
import { json, stubApi } from './support/stubApi'

test('marking the ticked invoices paid posts their ids and reloads the list', async ({ page }) => {
  // The invoices as the "database" holds them; the bulk call flips the ticked ones to Paid.
  const stored: Invoice[] = invoicesPage.items.map((invoice) => ({ ...invoice }))

  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      { method: HttpMethod.Get, path: '/api/billing', respond: () => ({ body: { ...invoicesPage, items: stored } }) },
      {
        method: HttpMethod.Post,
        path: '/api/billing/bulk',
        respond: (call) => {
          const { invoiceIds } = call.body as { invoiceIds: number[] }
          stored.forEach((invoice) => {
            if (invoiceIds.includes(invoice.id)) {
              invoice.status = 'Paid'
            }
          })
          return { status: 204 }
        },
      },
    ],
  })

  await page.goto('/billing/invoices')
  await expect(page.getByTestId('billing-invoices-table')).toBeVisible()
  await expect(page.getByTestId('billing-invoices-bulk')).toHaveCount(0)

  await page.getByTestId(`billing-invoice-select-${unpaidInvoice.id}`).check()
  await expect(page.getByTestId('billing-invoices-bulk')).toBeVisible()
  await page.getByTestId('billing-invoices-bulk-paid').click()

  await expect(page.getByTestId('billing-invoices-bulk')).toHaveCount(0)
  expect(api.callsTo(HttpMethod.Post, '/api/billing/bulk')[0]?.body).toEqual({ invoiceIds: [unpaidInvoice.id], action: 'MarkPaid' })
  // The reload after the action asked for the list a second time.
  expect(api.callsTo(HttpMethod.Get, '/api/billing').length).toBeGreaterThanOrEqual(2)
  await expect(page.getByTestId('billing-invoices-table')).toContainText('Paid')
})

test('deleting one invoice asks first and then removes it', async ({ page }) => {
  const stored: Invoice[] = [{ ...unpaidInvoice }, { ...paidInvoice }]

  const api = await stubApi(page, {
    signedIn: true,
    routes: [
      { method: HttpMethod.Get, path: '/api/billing', respond: () => ({ body: { ...invoicesPage, items: stored, totalCount: stored.length } }) },
      {
        method: HttpMethod.Delete,
        path: `/api/billing/${unpaidInvoice.id}`,
        respond: () => {
          stored.splice(0, 1)
          return { status: 204 }
        },
      },
    ],
  })

  await page.goto('/billing/invoices')
  await page.getByTestId(`billing-invoice-delete-${unpaidInvoice.id}`).click()
  // Nothing is sent until the dialog is confirmed.
  expect(api.callsTo(HttpMethod.Delete, `/api/billing/${unpaidInvoice.id}`)).toHaveLength(0)
  await page.getByTestId('dialog-confirm').click()

  await expect(page.getByTestId(`billing-invoice-delete-${unpaidInvoice.id}`)).toHaveCount(0)
  await expect(page.getByTestId(`billing-invoice-delete-${paidInvoice.id}`)).toBeVisible()
})

test('a refused load shows the API sentence', async ({ page }) => {
  const sentence = 'You are not allowed to list invoices.'
  await stubApi(page, {
    signedIn: true,
    routes: [json(HttpMethod.Get, '/api/billing', { type: 'FORBIDDEN', title: 'Forbidden', status: 403, detail: sentence }, 403)],
  })

  await page.goto('/billing/invoices')

  await expect(page.getByTestId('billing-invoices-error')).toContainText(sentence)
})
