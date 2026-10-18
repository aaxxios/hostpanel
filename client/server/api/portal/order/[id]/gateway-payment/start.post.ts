/**
 * POST /api/portal/order/:id/gateway-payment/start
 * Starts a hosted-gateway payment (e.g. Inecobank) for an order via the C# backend.
 * Body: { module: string, returnUrl: string }
 * Anonymous — the order/invoice already exist from checkout.
 *
 * @module server/api/portal/order/[id]/gateway-payment/start.post
 */

/** What the backend answers a gateway payment start with. */
interface GatewayStartResult {
  /** The bank page the browser is sent to. */
  redirectUrl: string
}

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<GatewayStartResult>(event, `/orders/${id}/gateway-payment/start`, {
    method: 'POST',
    body,
  })
})
