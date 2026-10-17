/**
 * Lifecycle status of an invoice, as `Innovayse.Domain.Billing.InvoiceStatus` serialises.
 *
 * @module types/clientinvoicestatus
 */

import type { InvoiceStatus } from '~/types/invoicestatus'

/**
 * Lifecycle status of an invoice, as `Innovayse.Domain.Billing.InvoiceStatus` serialises.
 *
 * `Program.cs` registers a `JsonStringEnumConverter`, so these arrive as the member names
 * verbatim. All eight members are listed: the previous union omitted `Overdue` and
 * `PaymentPending`, which are states the backend really does write, so a row in either one
 * type-checked as impossible while rendering in production.
 */
export type ClientInvoiceStatus = `${InvoiceStatus}`
