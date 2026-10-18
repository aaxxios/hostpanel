/**
 * What the API can and cannot say about the money charged for one service.
 *
 * @module types/serviceinvoices
 */

import type { ServiceInvoice } from '~/types/serviceinvoice'

/** What the API can and cannot say about the money charged for one service. */
export interface ServiceInvoices {
  /** Invoices carrying at least one line explicitly charged to this service, newest first. */
  invoices: ServiceInvoice[]
  /** How many of this client's invoices are recorded against no service at all. */
  unattributedInvoiceCount: number
}
