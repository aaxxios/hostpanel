/**
 * POST /api/portal/client/services/:id/change-password
 * Changes the hosting account password via the C# backend.
 *
 * Body: { password: string }
 *
 * @module server/api/portal/client/services/[id]/change-password.post
 */

import { HttpMethod } from '~/types/httpmethod'

/** Body this route accepts. */
interface ChangePasswordBody {
  /** The new hosting account password. */
  password?: string
}

export default defineEventHandler(async (event) => {
  const id = requireRouteParam(event, 'id')

  // No length check here: the backend validates the new password (`ChangePasswordValidator`,
  // minimum 8) and answers 400 `VALIDATION_ERROR` with the rule's sentence in the reader's language.
  const body = await readBody<ChangePasswordBody>(event)

  return await internalApiCall<Record<string, unknown>>(event, `/me/services/${id}/change-password`, {
    method: HttpMethod.Post,
    body: { newPassword: body.password },
  })
})
