/**
 * POST /api/portal/order/:id/confirm-payment
 * Confirms Stripe payment and auto-accepts the order.
 *
 * @module server/api/portal/order/[id]/confirm-payment.post
 */

/** What the backend answers a payment confirmation with. */
interface ConfirmPaymentResult {
  /** Whether the order was confirmed and accepted. */
  success: boolean
}

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<ConfirmPaymentResult>(event, `/orders/${id}/confirm-payment`, {
    method: 'POST',
    body,
  })
})
