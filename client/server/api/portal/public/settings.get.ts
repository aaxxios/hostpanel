/**
 * GET /api/portal/public/settings
 *
 * Returns the `portal.*` settings the storefront needs: the active template, the
 * operator-uploaded branding (logo, favicon) and the contact/newsletter widget
 * configuration.
 *
 * The key list below has to stay in step with `PortalSettingKeys.Public` in
 * `backend/src/Innovayse.Hostpanel.Domain/Settings/PortalSettingKeys.cs`, which is what
 * `GetPublicSettingsHandler` serves. This file cannot import that class, so the two
 * are kept in step by hand; `PUBLIC_PORTAL_SETTING_KEYS` is explicit and a test pins it to the backend set. A key the backend serves but this list omits is dropped
 * here without a trace, and the storefront renders its build-time default as though
 * the operator had never set the value.
 *
 * **Deliberately uncached.** A cache makes the admin panel appear broken: an operator uploads
 * a logo, reloads the storefront, sees the old one, and has no way to tell whether the
 * setting saved. With `swr` the first request after expiry is still answered from the stale
 * entry, so the wait would be the cache lifetime *and then* one more page load. A cache would
 * save one indexed read of a table with a few dozen rows, on a request that is already
 * talking to the same API for the page it is rendering. The response still transfers to the browser with the SSR payload,
 * so a visitor makes this call once per page render, not once per component that reads a
 * setting.
 *
 * Two things this deliberately does not do. It never returns the whole settings
 * table: that table holds integration credentials, and this endpoint is
 * unauthenticated. And it never fails the request: a backend that is down, or
 * an older backend with no public settings endpoint at all, yields an empty
 * object so the caller falls back to its environment defaults rather than the
 * site failing to render.
 */

import { PUBLIC_PORTAL_SETTING_KEYS } from '~/constants/publicPortalSettingKeys'

/** The only keys this endpoint will ever expose: an explicit list, never every enum member. */
const PUBLIC_KEYS: readonly string[] = PUBLIC_PORTAL_SETTING_KEYS

/** One public setting row, as the backend lists it. */
interface PublicSettingRow {
  /** Setting key, e.g. `portal.apps.mail`. */
  key: string
  /** Setting value. */
  value: string
}

/** Handler: the allow-listed, non-empty public settings as a key-to-value object; `{}` when the backend cannot answer. */
export default defineEventHandler(async (event) => {
  try {
    const rows = await internalApiCall<PublicSettingRow[]>(event, '/settings/public')

    const settings: Record<string, string> = {}
    for (const row of rows ?? []) {
      if (PUBLIC_KEYS.includes(row.key) && row.value) {
        settings[row.key] = row.value
      }
    }
    return settings
  } catch {
    // Backend unreachable, or too old to have this endpoint. Not an error here:
    // the storefront must render either way.
    return {}
  }
})
