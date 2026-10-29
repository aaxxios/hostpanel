/**
 * POST /api/portal/client/email-change/confirm
 * Makes the signed-in client's address change effective with the link's address and token.
 *
 * Body: { email: string, token: string }, passed through as is. On success the backend has ended
 * every session of the account, so the caller signs this browser out as well.
 *
 * @module server/api/portal/client/email-change/confirm.post
 */

import { HttpMethod } from '~/types/httpmethod'

/** Forwards the confirmation to the backend. */
export default defineEventHandler(async (event) => {
  const body = await readBody(event)
  return await internalApiCall<undefined>(event, '/clients/me/email-change/confirm', { method: HttpMethod.Post, body })
})
