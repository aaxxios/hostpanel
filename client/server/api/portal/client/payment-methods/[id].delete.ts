/**
 * DELETE /api/portal/client/payment-methods/:id
 * Removes a stored payment method via the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/payment-methods/${id}`, {
    method: 'DELETE',
  })
})
