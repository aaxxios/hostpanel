/**
 * POST /api/portal/auth/confirm-email
 *
 * Forwards an email-confirmation token to the C# API. Anonymous (confirmation happens before
 * there is a session), routed through `internalApiCall` so `X-Language` is forwarded and a
 * refusal keeps the backend's `detail`.
 *
 * The success body is empty and parses to an empty string: hence the `string` type argument;
 * the one caller (`useAuthApi.confirmEmail`) reads nothing from it.
 */

import { HttpMethod } from '~/types/httpmethod'
import { readBodyObject } from '~/server/utils/readBodyObject'

export default defineEventHandler(async (event) => {
  const body = await readBodyObject(event)

  return await internalApiCall<string>(event, '/auth/confirm-email', {
    method: HttpMethod.Post,
    body,
    anonymous: true,
  })
})
