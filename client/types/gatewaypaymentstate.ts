/**
 * Where a hosted-gateway payment stands on the return page, as reported by
 * `stores/invoicepayment.ts` `verifyGatewayPayment`.
 *
 * A closed set the page branches on, so it is an enum rather than a union of string literals.
 * `Paid`, `Pending` and `Declined` are only ever the backend's own word (the API sends them as
 * these exact strings, so a response's `state` is read as this enum without any mapping); `Unknown` means no authoritative answer could be had, and `Missing` that the
 * link carried nothing to verify.
 */
export enum GatewayPaymentState {
  /** The check is under way. */
  Verifying = 'Verifying',
  /** The bank confirmed the payment. */
  Paid = 'Paid',
  /** The bank has not settled the payment yet. */
  Pending = 'Pending',
  /** The bank declined the payment. */
  Declined = 'Declined',
  /** No authoritative answer could be obtained (network error, timeout, proxy failure). */
  Unknown = 'Unknown',
  /** The URL never carried an order or invoice id, so there is no payment attempt to check. */
  Missing = 'Missing'
}
