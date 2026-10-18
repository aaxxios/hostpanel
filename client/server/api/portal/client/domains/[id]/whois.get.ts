/**
 * GET /api/portal/client/domains/:id/whois
 * Returns the registry's WHOIS record of a domain (registrar, registrant, createdAt, expiresAt)
 * via the C# backend, forwarded as is.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/domains/${id}/whois`)
})
