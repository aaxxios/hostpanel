/**
 * POST /api/portal/client/email-change
 * Asks the C# backend to move the signed-in client's sign-in to a new address.
 *
 * Body: { newEmail: string, currentPassword: string }, passed through as is. The backend checks the
 * password and mails a confirmation link to the new address; nothing changes until it is followed.
 *
 * @module server/api/portal/client/email-change.post
 */

import { HttpMethod } from '~/types/httpmethod'
import type { EmailChangeInput } from '~/types/emailchangeinput'

/** Forwards the change request to the backend. */
export default defineEventHandler(async (event) => {
  const body = await readBody<EmailChangeInput>(event)
  return await internalApiCall<undefined>(event, '/clients/me/email-change', { method: HttpMethod.Post, body })
})
