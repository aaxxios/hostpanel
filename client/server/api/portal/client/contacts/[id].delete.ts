/**
 * DELETE /api/portal/client/contacts/:id
 * Removes a contact from the client's account via the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/clients/me/contacts/${id}`, {
    method: 'DELETE',
  })
})
