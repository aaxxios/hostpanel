/**
 * POST /api/portal/client/invoices/:id/pay
 * Initiates payment for an invoice via the C# backend.
 *
 * Body: { paymethodid?: number }
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, `/me/invoices/${id}/pay`, {
    method: 'POST',
    body,
  })
})
