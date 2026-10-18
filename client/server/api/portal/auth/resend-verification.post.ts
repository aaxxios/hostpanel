/**
 * POST /api/portal/auth/resend-verification
 *
 * Asks the C# API to send the signed-in account its email-verification mail again. Authenticated
 * (the session cookie becomes the bearer token inside `internalApiCall`), no body. The backend
 * answers 204 -- also when the address is already confirmed -- so the empty body parses to an
 * empty string, which is what the `string` type argument states. A refusal keeps the backend's
 * `detail`.
 */
export default defineEventHandler(async (event) => {
  return await internalApiCall<string>(event, '/auth/resend-verification', {
    method: 'POST',
  })
})
