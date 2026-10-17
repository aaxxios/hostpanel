/**
 * GET /api/portal/client/domains/:id/contact
 * Returns the WHOIS registrant contact the registrar currently holds for a domain, via the C#
 * backend, forwarded as is (the backend's `DomainContactDto`).
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  return await internalApiCall<Record<string, unknown>>(event, `/me/domains/${id}/contact`)
})
