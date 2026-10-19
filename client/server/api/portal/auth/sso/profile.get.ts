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

import { SessionCookie } from '~/constants/sessionCookie'
import { backendLanguage } from '~/server/utils/api'
import { buildClientMenu } from '~/server/utils/clientMenu'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  const accessToken = getCookie(event, SessionCookie.AuthToken)
  if (!accessToken) {
    throw createError({ statusCode: 401 })
  }

  const profile = await $fetch<Record<string, unknown>>(`${config.ssoUrl}/api/account/profile`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })

  // Extract ssoSub from JWT for account-switching (widget needs it to call /api/portal/auth/sso/switch)
  let ssoSub = (profile.sub as string | undefined) || ''
  if (!ssoSub) {
    try {
      const payload = accessToken.split('.')[1] ?? ''
      const decoded = JSON.parse(Buffer.from(payload, 'base64url').toString('utf8'))
      ssoSub = (decoded.sub as string | undefined) || ''
    } catch { /* ignore */ }
  }

  return {
    ...profile,
    ssoSub,
    menuItems: buildClientMenu(backendLanguage(event)),
  }
})
