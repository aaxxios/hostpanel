/**
 * PATCH /api/portal/client/me
 * Partially updates the authenticated client's profile via the C# backend.
 *
 * A pass-through: the body is already the backend's camelCase {@link ProfileUpdate}, and the
 * backend changes only the members it carries, so nothing is read, merged or renamed here.
 *
 * @module server/api/portal/client/me.patch
 */

import { HttpMethod } from '~/types/httpmethod'
import type { ProfileUpdate } from '~/types/profileupdate'

/** Forwards the profile changes to the backend and answers with the updated profile. */
export default defineEventHandler(async (event) => {
  const body = await readBody<ProfileUpdate>(event)

  return await internalApiCall<Record<string, unknown>>(event, '/clients/me', {
    method: HttpMethod.Patch,
    body,
  })
})
