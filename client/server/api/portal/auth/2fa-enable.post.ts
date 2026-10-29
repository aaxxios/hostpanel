/**
 * POST /api/portal/auth/2fa-enable
 * Switches two-factor authentication on for the authenticated account.
 *
 * Body: { code: string }: the digits the authenticator app currently shows.
 */

import { HttpMethod } from '~/types/httpmethod'
import { readBodyObject } from '~/server/utils/readBodyObject'

export default defineEventHandler(async (event) => {
  const body = await readBodyObject(event)
  return await internalApiCall<undefined>(event, '/me/2fa/enable', { method: HttpMethod.Post, body })
})
