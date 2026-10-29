/**
 * GET /api/portal/auth/sso/profile
 * Proxies to SSO GET /api/account/profile and returns user data.
 * Appends portal-specific menuItems so the widget can render
 * quick-links (Dashboard, Services, Domains, Invoices, Support) inside
 * the account popup under "Manage your Account". Each item carries `label` (already in the
 * reader's `X-Language`), the `client.nav.*` `labelKey`, `url` and an `icon` id.
 *
 * @module server/api/portal/auth/sso/profile.get
 */

import { backendLanguage } from '~/server/utils/api'
import { readTokenPayload } from '~/server/utils/auth-cookie'
import { resolveAccessToken } from '~/server/utils/portal-session'
import { signOnFetch } from '~/server/utils/signon-http'
import { buildClientMenu } from '~/server/utils/clientMenu'

export default defineEventHandler(async (event) => {
  const accessToken = await resolveAccessToken(event)
  if (!accessToken) {
    throw createError({ statusCode: 401 })
  }

  const profile = await signOnFetch<Record<string, unknown>>('/api/account/profile', { accessToken })

  // Extract ssoSub from JWT for account-switching (widget needs it to GET /api/portal/auth/sso/switch)
  let ssoSub = (profile.sub as string | undefined) || ''
  if (!ssoSub) {
    const sub = readTokenPayload(accessToken)?.['sub']
    ssoSub = typeof sub === 'string' ? sub : ''
  }

  return {
    ...profile,
    ssoSub,
    menuItems: buildClientMenu(backendLanguage(event)),
  }
})
