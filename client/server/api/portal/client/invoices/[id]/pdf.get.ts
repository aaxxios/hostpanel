/**
 * GET /api/portal/client/invoices/:id/pdf
 * Downloads one of the signed-in client's invoices as a PDF from the C# backend.
 *
 * The backend's `Cache-Control` is passed on, and `private, no-store` stands in when it sends
 * none: an invoice carries a client's billing details, so a missing header must not leave it
 * cacheable.
 */

/** What the download answers with when the backend names no caching rule of its own. */
const NO_STORE = 'private, no-store'

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const file = await internalApiBinaryCall(event, `/me/invoices/${encodeURIComponent(id)}/pdf`)
  setResponseHeaders(event, {
    'content-type': file.contentType,
    'cache-control': file.cacheControl ?? NO_STORE,
    ...(file.contentDisposition ? { 'content-disposition': file.contentDisposition } : {}),
  })
  return file.body
})
