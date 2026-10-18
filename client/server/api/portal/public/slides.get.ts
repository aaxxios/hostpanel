/**
 * GET /api/portal/public/slides
 *
 * Returns active slides for the current locale.
 * The language reaches the backend as `X-Language` (stamped by `internalApiCall`).
 * Optionally accepts `audience` query param (guest/authenticated).
 */
export default defineEventHandler(async (event) => {
  const query = getQuery(event)
  const audienceParam = query.audience ? `?audience=${query.audience}` : ''

  return await internalApiCall(event, `/slides${audienceParam}`)
})
