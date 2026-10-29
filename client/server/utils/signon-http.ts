/**
 * The one HTTP client for the external sign-on service.
 *
 * Every server route that talks to it (profile reads, the refresh-token exchange) goes through
 * `signOnFetch`, so each call has a timeout and a failure reaches the browser as a typed error
 * carrying a `data.code` and nothing else. A bare `$fetch` composes a message that embeds the
 * address it called, which would publish the internal host of a service the browser cannot reach.
 *
 * Mapping of a failure: a 4xx answer keeps its status (the caller may act on 400/401/403),
 * a timeout is 504, anything else (no answer, a 5xx) is 502.
 *
 * @module server/utils/signon-http
 */

import { HttpMethod } from '~/types/httpmethod'
import { SignOnErrorCode } from '~/types/signonerrorcode'

/** How long the sign-on service has to answer, in milliseconds. */
export const SIGN_ON_TIMEOUT_MS = 10_000

/** Options of one `signOnFetch` call. */
export interface SignOnFetchOptions {
  /** HTTP method; GET when absent. */
  method?: HttpMethod
  /** Extra request headers. */
  headers?: Record<string, string>
  /** Request body, already serialised (the token endpoint takes a form string). */
  body?: string
  /** Access token sent as the bearer, when the call is made on a person's behalf. */
  accessToken?: string
}

/** What a failed `$fetch` exposes that the mapping reads. */
interface SignOnFailure {
  /** HTTP status (ofetch). */
  status?: number
  /** HTTP status (H3-style). */
  statusCode?: number
  /** The error name; `TimeoutError` when the request ran out of time. */
  name?: string
  /** The underlying error, which carries the timeout name when ofetch wraps it. */
  cause?: { name?: string }
}

/**
 * Whether a failure is the request running out of time.
 *
 * @param failure - Whatever `$fetch` threw.
 * @returns True for a timeout.
 */
const isTimeout = (failure: SignOnFailure): boolean =>
  failure?.name === 'TimeoutError' || failure?.cause?.name === 'TimeoutError' || failure?.cause?.name === 'AbortError'

/**
 * Turns a failed sign-on call into the typed error the caller rethrows.
 *
 * @param err - Whatever `$fetch` threw.
 * @returns An H3 error with a status and `data.code`, and no message.
 */
const signOnError = (err: unknown) => {
  const failure = err as SignOnFailure
  const status = failure?.status ?? failure?.statusCode
  if (typeof status === 'number' && status >= 400 && status < 500) {
    return createError({ statusCode: status, data: { code: SignOnErrorCode.Refused } })
  }
  if (isTimeout(failure)) {
    return createError({ statusCode: 504, data: { code: SignOnErrorCode.Timeout } })
  }
  return createError({ statusCode: 502, data: { code: SignOnErrorCode.Unavailable } })
}

/**
 * Calls the sign-on service.
 *
 * @param path - The path on the service, starting with `/`.
 * @param options - Method, headers, body and the bearer.
 * @returns The parsed reply.
 * @throws H3Error with `data.code` (a `SignOnErrorCode`) and no message when the call fails.
 */
export const signOnFetch = async <T>(path: string, options: SignOnFetchOptions = {}): Promise<T> => {
  const config = useRuntimeConfig()
  // Bound to a plain `string`: handed a template literal, Nitro tries to match it against its own
  // route table and the inferred reply type recurses (TS2321).
  const url: string = `${config.ssoUrl}${path}`
  const headers: Record<string, string> = { ...options.headers }
  if (options.accessToken) {
    headers['Authorization'] = `Bearer ${options.accessToken}`
  }
  try {
    // The request type is stated as a plain `string`: left to its default, `$fetch` types the
    // reply as a union over every Nitro route, although this address is the sign-on service's.
    return await $fetch<T, string>(url, {
      method: options.method ?? HttpMethod.Get,
      headers,
      body: options.body,
      timeout: SIGN_ON_TIMEOUT_MS,
      retry: 0,
    })
  } catch (err: unknown) {
    throw signOnError(err)
  }
}
