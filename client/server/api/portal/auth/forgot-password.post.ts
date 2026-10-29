/**
 * POST /api/portal/auth/forgot-password
 *
 * Forwards a password-reset request to the C# API, which emails the link. Anonymous (no session
 * yet), routed through `internalApiCall` so `X-Language` is forwarded.
 *
 * The backend answers `Ok()` with an empty body whether or not the address belongs to an
 * account: deliberately, so this endpoint cannot be used to enumerate users. There is
 * therefore nothing to read back; the empty body parses to an empty string, which is what the
 * `string` type argument states.
 */

import { HttpMethod } from '~/types/httpmethod'
import { readBodyObject } from '~/server/utils/readBodyObject'

export default defineEventHandler(async (event) => {
  const body = await readBodyObject(event)

  return await internalApiCall<string>(event, '/auth/forgot-password', {
    method: HttpMethod.Post,
    body,
    anonymous: true,
  })
})
