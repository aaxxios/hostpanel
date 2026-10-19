/**
 * Post-sign-in redirect target: the query parameter that carries it and the validator that decides
 * whether a requested value may be navigated to. Shared by the auth guard (which writes it) and the
 * sign-in screen (which reads it), so both agree on the name and on what is acceptable.
 */
import { AppRoute } from '@/lib/routes'

/** Name of the query parameter that carries the page the visitor originally asked for. */
export const REDIRECT_QUERY_PARAM = 'redirect'

/** Origin used only to resolve a candidate path; never navigated to. */
const PROBE_ORIGIN = 'http://redirect-probe.invalid'

/** Matches any ASCII control character or backslash, which browsers may normalise into a `/`. */
const UNSAFE_CHARACTERS = /[\u0000-\u001f\u007f\\]/

/** Screens a sign-in must never bounce back to, or the visitor would loop. */
const EXCLUDED_PATHS: readonly string[] = [AppRoute.Login, AppRoute.Setup]

/**
 * Accepts a redirect target only when it is a relative path inside this app.
 *
 * Rejects anything that is not a string, does not start with a single `/`, starts with `//`
 * (protocol-relative), contains a scheme, backslash or control character, resolves to another
 * origin, or points at the sign-in or setup screens.
 *
 * @param value - Raw candidate, typically a query-string value.
 * @returns The safe path (with query and hash), or null when the value must be ignored.
 */
export const safeRedirectTarget = (value: unknown): string | null => {
  if (typeof value !== 'string') {
    return null
  }
  if (!value.startsWith('/') || value.startsWith('//') || UNSAFE_CHARACTERS.test(value)) {
    return null
  }

  let resolved: URL
  try {
    resolved = new URL(value, PROBE_ORIGIN)
  } catch {
    return null
  }
  if (resolved.origin !== PROBE_ORIGIN || EXCLUDED_PATHS.includes(resolved.pathname)) {
    return null
  }

  return value
}

/**
 * Builds the sign-in location that remembers where the visitor was heading.
 *
 * @param fullPath - Full path (with query and hash) of the page that was requested.
 * @returns The login path, with a `redirect` query when the target is acceptable.
 */
export const loginLocationFor = (fullPath: string): string => {
  const target = safeRedirectTarget(fullPath)
  if (target === null) {
    return AppRoute.Login
  }
  return `${AppRoute.Login}?${REDIRECT_QUERY_PARAM}=${encodeURIComponent(target)}`
}
