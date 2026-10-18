/**
 * DELETE /api/portal/client/users/:id
 * Removes a user's access to the authenticated client account via the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/users/${id}`, {
    method: 'DELETE',
  })
})
