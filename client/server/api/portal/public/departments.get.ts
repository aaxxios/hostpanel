/**
 * GET /api/portal/public/departments
 * Returns active support departments from the C# backend.
 */
export default defineEventHandler(async (event) => {
  // No try/catch swallowing the failure: a refusal turned into an empty array would leave
  // the ticket form's department picker empty with nothing in the logs to say why.
  return await internalApiCall<unknown[]>(event, '/departments')
})
