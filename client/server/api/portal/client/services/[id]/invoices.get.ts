/**
 * GET /api/portal/client/services/:id/invoices
 *
 * Returns the invoices charged to one of the authenticated client's own services.
 *
 * `InvoiceItem.ClientServiceId` records the link between an invoice line and a service where it
 * is known, and `GET /me/services/{id}/invoices` reads it, scoped to the caller's own
 * account by `GetMyServiceInvoicesQuery`, which resolves the client from the credential and
 * refuses a service that is not theirs with the same 404 every other client-facing refusal uses.
 *
 * **The reply is not a bare list, and must not be flattened into one.** It carries
 * `unattributedInvoiceCount` beside `invoices`: lines that predate the column have no link, and
 * inferring which service an old line was for from its description text would be a guess
 * rendered as fact on a page a customer uses to check a charge. An empty
 * `invoices` array therefore means "nothing is recorded against this service", which is a weaker
 * claim than "nothing was charged", and the page has to be able to tell the two apart.
 */

/** One invoice, as `GetMyServiceInvoicesQuery` returns it. */
interface ServiceInvoice {
  /** Invoice primary key. */
  id: number
  /** Issue date (UTC, ISO 8601). */
  invoiceDate: string
  /** Payment due date (UTC, ISO 8601). */
  dueDate: string
  /** Grand total after tax and credit. */
  total: number
  /** Lifecycle status name, e.g. `Unpaid`, `Paid`, `Overdue`. */
  status: string
}

/** What the platform can and cannot say about the money charged for one service. */
interface ServiceInvoices {
  /** Invoices carrying at least one line explicitly charged to this service, newest first. */
  invoices: ServiceInvoice[]
  /** How many of the caller's invoices are recorded against no service at all. */
  unattributedInvoiceCount: number
}

export default defineEventHandler(async (event): Promise<ServiceInvoices> => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<ServiceInvoices>(event, `/me/services/${id}/invoices`)
})
