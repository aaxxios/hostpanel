/**
 * One entry of the payment-gateway log, the outcomes an entry can carry, and the filters the log
 * is listed by.
 */

/** Outcomes a gateway log entry reports; the values are the names the API sends. */
export enum GatewayLogResult {
  Payment = 'Payment',
  Refund = 'Refund',
  Credit = 'Credit',
  Pending = 'Pending',
}

/** A single recorded gateway interaction: a settled transaction or an open payment session. */
export interface GatewayLogEntry {
  /** Unique entry key (`tx-12` or `session-7`). */
  id: string
  /** ISO 8601 time of the interaction. */
  dateTime: string
  /** Name of the gateway. */
  gateway: string
  /** Invoice, gateway reference and notes, as text. */
  debugData: string
  /** How the interaction ended. */
  result: GatewayLogResult
  /** The invoice the entry belongs to, when it has one. */
  invoiceId: number | null
}

/** Filters the gateway log can be listed by; an empty value means no filter. */
export interface GatewayLogFilters {
  /** Inclusive start day (ISO date), or empty. */
  from: string
  /** Inclusive end day (ISO date), or empty. */
  to: string
  /** Gateway name text, or empty. */
  gateway: string
  /** Debug text, or empty. */
  debugData: string
  /** Outcome, or empty for any. */
  result: GatewayLogResult | ''
}
