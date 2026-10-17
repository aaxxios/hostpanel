/**
 * POST /api/portal/auth/reset-password
 *
 * Forwards a new password and its reset token to the C# API. Anonymous (the caller is by
 * definition signed out), routed through `internalApiCall` so `X-Language` is forwarded and a
 * refusal keeps the backend's `detail`.
 *
 * The success body is empty and parses to an empty string -- hence the `string` type argument.
 */
export default defineEventHandler(async (event) => {
  const body = await readBody(event)

  return await internalApiCall<string>(event, '/auth/reset-password', {
    method: 'POST',
    body,
    anonymous: true,
  })
})
