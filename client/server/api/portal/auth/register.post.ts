/**
 * POST /api/portal/auth/register
 *
 * Forwards a sign-up to the C# API. Anonymous (there is no account yet, so no bearer token), but
 * routed through `internalApiCall` so `X-Language` is forwarded and a refusal keeps the
 * backend's `detail`, `code` and field `errors`.
 *
 * No field is checked here: the backend decides whether the registration is acceptable.
 *
 * @module server/api/portal/auth/register.post
 */

/** What the backend answers a sign-up with. */
interface RegisterResult {
  /** Identifier of the account that was created. */
  userId: string
}

/**
 * Forwards the sign-up to the backend.
 */
export default defineEventHandler(async (event) => {
  const body = await readBody(event)

  return await internalApiCall<RegisterResult>(event, '/auth/register', {
    method: 'POST',
    body,
    anonymous: true,
  })
})
