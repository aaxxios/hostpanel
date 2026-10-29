/**
 * POST /api/portal/client/users/invite
 * Invites a new user to the authenticated client account via the C# backend.
 *
 * Body: {@link InviteUserPayload}; `permissionMode` is an `InvitePermissionMode` and
 * `permissions` the chosen `TeamPermission` values (empty for `InvitePermissionMode.All`).
 *
 * @module server/api/portal/client/users/invite.post
 */

import { HttpMethod } from '~/types/httpmethod'
import type { InviteUserPayload } from '~/types/inviteuserpayload'

/** Forwards the invitation to the backend and answers with what it returned. */
export default defineEventHandler(async (event) => {
  const body = await readBody<InviteUserPayload>(event)
  return await internalApiCall<Record<string, unknown>>(event, '/me/users/invite', {
    method: HttpMethod.Post,
    body,
  })
})
