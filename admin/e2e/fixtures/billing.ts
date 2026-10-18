/**
 * Fixtures for creating an invoice from a client's invoices tab.
 */
import type { Invoice } from '../../src/types/invoice'
import type { PagedResult } from '../../src/types/pagedresult'

/** Identifier the stubbed `POST /api/billing` hands out. */
export const newInvoiceId = 501

/** The invoice that exists once the create call has been answered. */
export const createdInvoice: Invoice = {
  id: newInvoiceId,
  clientId: 7,
  status: 'Draft',
  dueDate: '2026-12-31T00:00:00Z',
  createdAt: '2026-10-14T09:00:00Z',
  invoiceDate: '2026-10-14T00:00:00Z',
  total: 240,
  subTotal: 240,
  tax: 0,
  taxRate: 0,
  credit: 0,
  clientName: 'Ada Lovelace',
  items: [{ id: 1, description: 'Managed hosting', unitPrice: 120, quantity: 2, amount: 240 }],
  transactions: [],
}

/** `GET /api/billing/client/7` before anything was created. */
export const emptyClientInvoices: PagedResult<Invoice> = { items: [], totalCount: 0, page: 1, pageSize: 25 }
