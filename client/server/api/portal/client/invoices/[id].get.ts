/**
 * GET /api/portal/client/invoices/:id
 * Returns full details of a single invoice from the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/invoices/${id}`)
})
