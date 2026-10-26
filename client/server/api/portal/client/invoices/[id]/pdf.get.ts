/**
 * GET /api/portal/client/invoices/:id/pdf
 * Downloads one of the signed-in client's invoices as a PDF from the C# backend.
 */
export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  const file = await internalApiBinaryCall(event, `/me/invoices/${id}/pdf`)
  setResponseHeaders(event, {
    'content-type': file.contentType,
    ...(file.contentDisposition ? { 'content-disposition': file.contentDisposition } : {}),
  })
  return file.body
})
