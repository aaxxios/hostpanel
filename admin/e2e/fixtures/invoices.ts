/**
 * Fixtures for the invoices list: one unpaid and one paid invoice on a single page.
 */
import type { Invoice } from '../../src/types/invoice'
import type { PagedResult } from '../../src/types/pagedresult'

/** An invoice the operator can mark paid. */
export const unpaidInvoice: Invoice = {
  id: 601,
  clientId: 7,
  status: 'Unpaid',
  dueDate: '2026-11-30T00:00:00Z',
  createdAt: '2026-10-01T09:00:00Z',
  invoiceDate: '2026-10-01T00:00:00Z',
  total: 120,
  subTotal: 120,
  tax: 0,
  taxRate: 0,
  credit: 0,
  clientName: 'Ada Lovelace',
  items: [{ id: 1, description: 'Hosting', unitPrice: 120, quantity: 1, amount: 120 }],
  transactions: [],
}

/** An invoice that is already settled. */
export const paidInvoice: Invoice = {
  ...unpaidInvoice,
  id: 602,
  clientId: 8,
  status: 'Paid',
  clientName: 'Grace Hopper',
  total: 60,
  subTotal: 60,
}

/** `GET /api/billing` — one page holding both invoices. */
export const invoicesPage: PagedResult<Invoice> = {
  items: [unpaidInvoice, paidInvoice],
  totalCount: 2,
  page: 1,
  pageSize: 25,
}
