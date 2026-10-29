/**
 * The one check for a URL that came from data (content, a backend record) before it is put into
 * an `href` or `src`.
 *
 * Operator and backend values are data, not code. Even where the backend already validates them,
 * the page does not trust the string: `javascript:`, `data:` and other schemes are refused here.
 *
 * @module utils/safeUrl
 */

/** URL schemes a plain link may use. */
const WEB_SCHEMES: ReadonlySet<string> = new Set(['http', 'https'])

/**
 * Whether a URL is safe for the given tag.
 *
 * Control characters and whitespace are removed before the scheme is read, because a browser
 * ignores them inside a scheme (`java\tscript:`), and a URL without a scheme (relative, `#`,
 * `?`) is allowed.
 *
 * @param url The attribute value, already entity-decoded.
 * @param schemes The schemes the attribute may use.
 * @returns True when the URL is relative or uses an allowed scheme.
 */
export const isSafeUrl = (url: string, schemes: ReadonlySet<string>): boolean => {
  const compact = Array.from(url).filter((char) => {
    const code = char.charCodeAt(0)
    return code > 0x20 && (code < 0x7f || code > 0x9f)
  }).join('')
  if (compact === '') {
    return false
  }
  const scheme = /^([a-zA-Z][a-zA-Z0-9+.-]*):/.exec(compact)
  if (scheme) {
    return schemes.has((scheme[1] ?? '').toLowerCase())
  }
  // `//host/path` is protocol-relative; it inherits the page scheme, which is safe.
  return true
}

/**
 * Passes a web link through only when it is safe to put into an `href`.
 *
 * @param url The address from content or a backend record; may be absent.
 * @returns The same address when it is relative or uses `http` or `https`, otherwise `undefined`
 * so the caller renders no link.
 */
export const safeHref = (url: string | null | undefined): string | undefined =>
  url && isSafeUrl(url, WEB_SCHEMES) ? url : undefined
