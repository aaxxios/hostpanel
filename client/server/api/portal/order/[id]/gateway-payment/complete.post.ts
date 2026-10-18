/**
 * POST /api/portal/order/:id/gateway-payment/complete
 * Verifies a hosted-gateway payment for an order against the C# backend,
 * which pulls the authoritative status from the bank.
 * Anonymous — matches the start endpoint's auth model.
 *
 * @module server/api/portal/order/[id]/gateway-payment/complete.post
 */

import type { GatewayPaymentState } from '~/types/gatewaypaymentstate'

/** What the backend answers a gateway verification with. */
interface GatewayPaymentResult {
  /** Where the payment stands, as the bank reported it. */
  state: GatewayPaymentState.Paid | GatewayPaymentState.Pending | GatewayPaymentState.Declined
}

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<GatewayPaymentResult>(
    event, `/orders/${id}/gateway-payment/complete`, { method: 'POST' })
})
