/**
 * Lifecycle status of an invoice, as `Innovayse.Domain.Billing.InvoiceStatus` serialises.
 *
 * `Program.cs` registers a `JsonStringEnumConverter`, so these arrive as the member names
 * verbatim. All eight members are listed.
 */
export enum InvoiceStatus {
  /** Not yet issued. */
  Draft = 'Draft',
  /** Issued, awaiting payment. */
  Unpaid = 'Unpaid',
  /** Settled. */
  Paid = 'Paid',
  /** Unpaid past its due date, as written by the renewals job. */
  Overdue = 'Overdue',
  /** Voided. */
  Cancelled = 'Cancelled',
  /** Paid and then returned. */
  Refunded = 'Refunded',
  /** Handed to collections. */
  Collections = 'Collections',
  /** A payment is in flight. */
  PaymentPending = 'PaymentPending'
}
