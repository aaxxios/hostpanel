/**
 * Fail-closed guard for the single sign-on addresses the Nitro routes build URLs from.
 *
 * `ssoPublicUrl` and `ssoCallbackUrl` default to empty. A route that interpolated an empty value
 * would redirect the browser to a broken relative URL, so a missing address is a coded 503 and
 * no sentence (the browser renders the generic title; the BFF writes no prose).
 *
 * @module server/utils/requireSsoSetting
 */

/** The `data.code` of the 503 an unconfigured single sign-on address answers with. */
export const SSO_NOT_CONFIGURED = 'SSO_NOT_CONFIGURED'

/**
 * Returns a configured single sign-on address, or refuses the request when it is empty.
 *
 * @param value - The runtime-config value (`ssoPublicUrl`, `ssoCallbackUrl`).
 * @returns The value, never empty.
 * @throws H3Error 503 carrying `data.code = SSO_NOT_CONFIGURED` and no `statusMessage`.
 */
export const requireSsoSetting = (value: unknown): string => {
  if (typeof value !== 'string' || value.trim() === '') {
    throw createError({ statusCode: 503, data: { code: SSO_NOT_CONFIGURED } })
  }
  return value
}
