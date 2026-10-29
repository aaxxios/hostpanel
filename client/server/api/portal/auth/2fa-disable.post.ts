/**
 * POST /api/portal/auth/2fa-disable
 * Switches two-factor authentication off for the authenticated account.
 *
 * Body: { code: string, currentPassword: string }. A code and the password are required rather
 * than just a signed-in session: removing the second factor is exactly what someone who had
 * taken over a session would want to do. Switching it off ends every session of the account.
 */

import { HttpMethod } from '~/types/httpmethod'
import { readBodyObject } from '~/server/utils/readBodyObject'

export default defineEventHandler(async (event) => {
  const body = await readBodyObject(event)
  return await internalApiCall<undefined>(event, '/me/2fa/disable', { method: HttpMethod.Post, body })
})
