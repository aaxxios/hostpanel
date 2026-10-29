/**
 * What the backend answers a hosted-gateway payment verification with.
 *
 * @module types/gatewaycompletion
 */

import type { GatewayPaymentState } from '~/types/gatewaypaymentstate'

/** Where a hosted-gateway payment stands once the backend has asked the bank. */
export interface GatewayCompletion {
  /** The backend's own word for the payment: paid, still pending, or declined. */
  state: GatewayPaymentState.Paid | GatewayPaymentState.Pending | GatewayPaymentState.Declined
  /** The order's own number (for example `ORD-0042`); present only when the payment was for an order. */
  orderNumber?: string
}
