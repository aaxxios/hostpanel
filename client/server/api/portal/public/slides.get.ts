/**
 * GET /api/portal/public/slides
 *
 * Returns active slides for the current locale.
 * The language reaches the backend as `X-Language` (stamped by `internalApiCall`). Which audiences
 * are included is decided by the backend from the visitor's session, which `internalApiCall` forwards;
 * the request carries no audience of its own.
 */
export default defineEventHandler(async (event) => {
  return await internalApiCall(event, '/slides')
})
