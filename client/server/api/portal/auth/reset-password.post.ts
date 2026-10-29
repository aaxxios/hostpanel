/**
 * POST /api/portal/auth/reset-password
 *
 * Forwards a new password and its reset token to the C# API. Anonymous (the caller is by
 * definition signed out), routed through `internalApiCall` so `X-Language` is forwarded and a
 * refusal keeps the backend's `detail`.
 *
 * The success body is empty and parses to an empty string: hence the `string` type argument.
 */

import { HttpMethod } from '~/types/httpmethod'
import { readBodyObject } from '~/server/utils/readBodyObject'

export default defineEventHandler(async (event) => {
  const body = await readBodyObject(event)

  return await internalApiCall<string>(event, '/auth/reset-password', {
    method: HttpMethod.Post,
    body,
    anonymous: true,
  })
})
