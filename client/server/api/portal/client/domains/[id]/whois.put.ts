/**
 * PUT /api/portal/client/domains/:id/whois
 * Updates the WHOIS registrant contact of a domain via the C# backend.
 *
 * Body: { contact: { firstName, lastName, organization, email, phone, address1, address2, city,
 * state, postalCode, country } }: the backend's `ModifyMyDomainContactCommand`, forwarded as is.
 */

import { HttpMethod } from '~/types/httpmethod'

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const body = await readBody(event)
  return await internalApiCall<Record<string, unknown>>(event, `/me/domains/${id}/whois`, {
    method: HttpMethod.Put,
    body,
  })
})
