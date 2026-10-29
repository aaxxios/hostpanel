/**
 * The one place the browser is handed to an address outside this site.
 *
 * A payment gateway's address comes from the backend, which got it from a plugin. It is data, not
 * code, so it is checked here before the browser follows it: only an absolute `https` address
 * qualifies, plus `http` while the app runs in development, where a local stand-in gateway has no
 * certificate. `javascript:`, `data:` and relative or malformed values are refused.
 *
 * @module utils/externalRedirect
 */

import { HTTPS_PREFIX } from '~/constants/urlScheme'

/** Scheme prefix of an insecure address, accepted in development only. */
const HTTP_PREFIX = 'http://'

/**
 * Whether the browser may be handed to an address.
 *
 * @param target - The address the backend returned.
 * @param allowHttp - Whether `http` is acceptable; true only in development.
 * @returns True when the address is absolute and uses an accepted scheme.
 */
export const isSafeRedirectUrl = (target: string, allowHttp: boolean = Boolean(import.meta.dev)): boolean => {
  let parsed: URL
  try {
    parsed = new URL(target)
  } catch {
    return false
  }
  const prefix = `${parsed.protocol}//`
  return prefix === HTTPS_PREFIX || (allowHttp && prefix === HTTP_PREFIX)
}

/**
 * Hands the browser to an external address, when it is a safe one.
 *
 * @param target - The address to leave for.
 * @returns True when the browser was handed over; false when the address was refused and nothing happened.
 */
export const redirectToExternal = (target: string): boolean => {
  if (!isSafeRedirectUrl(target)) {
    return false
  }
  window.location.href = target
  return true
}
