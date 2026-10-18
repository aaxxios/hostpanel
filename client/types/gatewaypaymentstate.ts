/**
 * Where a hosted-gateway payment stands on the return page, as reported by
 * `stores/invoicepayment.ts` `verifyGatewayPayment`.
 *
 * A closed set the page branches on, so it is an enum rather than a union of string literals.
 * `Paid`, `Pending` and `Declined` are only ever the backend's own word (the API sends them as
 * these strings); `Unknown` means no authoritative answer could be had, and `Missing` that the
 * link carried nothing to verify.
 */
export enum GatewayPaymentState {
  /** The check is under way. */
  Verifying = 'verifying',
  /** The bank confirmed the payment. */
  Paid = 'paid',
  /** The bank has not settled the payment yet. */
  Pending = 'pending',
  /** The bank declined the payment. */
  Declined = 'declined',
  /** No authoritative answer could be obtained (network error, timeout, proxy failure). */
  Unknown = 'unknown',
  /** The URL never carried an order or invoice id, so there is no payment attempt to check. */
  Missing = 'missing'
}
