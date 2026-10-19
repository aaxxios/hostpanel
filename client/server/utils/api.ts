/**
 * C# backend API utility -- server-side only
 *
 * All backend API calls go through `internalApiCall`. It:
 * - Reads `apiUrl` from runtimeConfig (set via API_URL env var)
 * - Attaches the JWT access token from the `auth_token` httpOnly cookie
 * - Forwards the browser's `X-Language` on every backend call (the retry after a refresh included)
 *   so the backend answers its sentences in the reader's language
 * - Sliding session: when the backend answers with a renewed access token
 *   (`X-Renewed-Access-Token`), stores it in the `auth_token` cookie so the session outlives
 *   the 15-minute token for as long as the person keeps working
 * - On 401: silently refreshes the token using the `refresh_token` cookie
 *   and retries the request once
 * - Throws appropriate H3 errors on failure; the sentence is always the backend's own `detail`
 *
 * @module server/utils/api
 */

import type { H3Event } from 'h3'
import { LANGUAGE_HEADER, resolveLanguage } from '~/utils/transportLanguage'
import { SessionCookie } from '~/constants/sessionCookie'
import { ACCESS_TOKEN_FALLBACK_MAX_AGE, AUTHED_FLAG_MAX_AGE } from '~/server/utils/auth-cookie'

/** The SSO token endpoint's reply to a refresh-token grant. */
interface RefreshTokenResponse {
  /** The new access token. */
  access_token: string
  /** The rotated refresh token, when the SSO issued one. */
  refresh_token?: string
  /** Lifetime of the new access token, in seconds. */
  expires_in?: number
}

/** What a failed backend call exposes that `backendError` reads. */
interface UpstreamError {
  /** The backend's JSON body, when it sent one. */
  data?: unknown
}

/** Options of one `internalApiCall`. */
interface InternalApiOptions {
  /** HTTP method; GET when absent. */
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE' | 'PATCH'
  // `unknown` before, which `$fetch` will not accept as a body -- every caller passes an
  // object literal, so this states what they already send rather than widening at the call.
  /** Request body. */
  body?: Record<string, unknown> | BodyInit | null
  /** Extra request headers. */
  headers?: Record<string, string>
  /** Omit the bearer token, for the calls made before a session exists. */
  anonymous?: boolean
}

/** Response header the backend uses to hand a bearer caller a renewed access token. */
const RENEWED_TOKEN_HEADER = 'x-renewed-access-token'

/** Module-level lock to prevent concurrent refresh calls. */
let refreshPromise: Promise<string | null> | null = null

/**
 * Resolve the C# backend base URL from runtime config.
 *
 * @returns The backend API base URL (`API_URL`, defaulted in `nuxt.config.ts`).
 */
export const getApiUrl = (): string => {
  const config = useRuntimeConfig()
  return config.apiUrl as string
}

/**
 * Resolves the language the backend should answer in for this request.
 *
 * Reads the `X-Language` header the browser-side `useApi` stamps and validates it against
 * `en|ru|hy`, so an arbitrary header value never reaches the backend.
 *
 * @param event - H3 event of the incoming browser request.
 * @returns A supported language code, the i18n default when the header is missing or unsupported.
 */
export const backendLanguage = (event: H3Event): string => {
  return resolveLanguage(getHeader(event, LANGUAGE_HEADER))
}

/**
 * Attempt to refresh the access token using the stored refresh token.
 *
 * Calls the SSO token endpoint with grant_type=refresh_token. On success,
 * updates all auth cookies and returns the new access token. On failure,
 * clears all cookies and returns null.
 *
 * Uses a module-level promise lock so concurrent 401s share a single refresh call.
 *
 * @param event - H3 event (needed to read/write cookies on the browser response)
 * @returns The new access token string, or null if refresh failed.
 */
export const tryRefreshToken = async (event: H3Event): Promise<string | null> => {
  if (refreshPromise) {
    return refreshPromise
  }

  const refreshToken = getCookie(event, SessionCookie.RefreshToken)
  if (!refreshToken) {
    clearAllAuthCookies(event)
    return null
  }

  refreshPromise = (async () => {
    try {
      const config = useRuntimeConfig()
      const response = await $fetch<RefreshTokenResponse>(`${config.ssoUrl}/connect/token`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: new URLSearchParams({
          grant_type: 'refresh_token',
          refresh_token: refreshToken,
          client_id: config.ssoClientId,
          client_secret: config.ssoClientSecret,
        }).toString(),
      })

      const { access_token, refresh_token: newRefreshToken, expires_in } = response
      if (!access_token) {
        clearAllAuthCookies(event)
        return null
      }

      setCookie(event, SessionCookie.AuthToken, access_token, {
        httpOnly: true,
        secure: process.env.NODE_ENV === 'production',
        sameSite: 'lax',
        maxAge: expires_in ?? ACCESS_TOKEN_FALLBACK_MAX_AGE,
        path: '/',
      })

      if (newRefreshToken) {
        setCookie(event, SessionCookie.RefreshToken, newRefreshToken, {
          httpOnly: true,
          secure: process.env.NODE_ENV === 'production',
          sameSite: 'lax',
          maxAge: REFRESH_TOKEN_MAX_AGE,
          path: '/',
        })
      }

      return access_token
    } catch {
      clearAllAuthCookies(event)
      return null
    }
  })()

  try {
    return await refreshPromise
  } finally {
    refreshPromise = null
  }
}

/** The problem+json `ErrorResponse` every C# backend endpoint answers a refusal with. */
interface BackendErrorBody {
  /** The machine-readable SCREAMING_SNAKE code, e.g. `CLIENT_PROFILE_NOT_FOUND`. Never localized. */
  type?: string
  /** Short localized summary; the sentence of last resort when `detail` is absent. */
  title?: string
  /** The human-readable sentence, localized by the backend. Rendered as-is. */
  detail?: string
  /** Field name to messages, present on a 400 `VALIDATION_ERROR`. */
  errors?: Record<string, string[]>
}

/**
 * Turns a failed backend call into the H3 error this route rethrows to the browser.
 *
 * The sentence (`detail`, else `title`) travels in `statusMessage`; the backend's code (`type`),
 * its `title` and its field `errors` travel in `data`, because `data` is the only other part of an
 * H3Error that `server/error.ts` serialises into the JSON body the browser receives. The browser
 * therefore never has to string-match English prose to tell one refusal from another.
 *
 * When the backend sent no body at all (it was unreachable, or answered a bare status) there is
 * no sentence to forward and none is invented here: `statusMessage` stays empty and the client's
 * one "no answer" sentence is shown. `err.message` is deliberately not a fallback: on a response
 * with no JSON body -- a bare 401 above all -- `$fetch` composes its own message and embeds the
 * address it called, which would publish the internal host and port of a service the browser
 * cannot reach.
 *
 * @param status - HTTP status the backend answered with.
 * @param err - The `$fetch` rejection, whose `data` holds the backend's JSON body.
 * @returns An H3 error ready to be thrown.
 */
const backendError = (status: number, err: UpstreamError) => {
  const data = err?.data as BackendErrorBody | undefined

  return createError({
    statusCode: status,
    statusMessage: data?.detail ?? data?.title,
    data: {
      code: data?.type ?? null,
      title: data?.title ?? null,
      errors: data?.errors ?? null,
    },
  })
}

/** What a `$fetch` rejection exposes about the upstream answer. */
interface UpstreamFailure {
  /** HTTP status (ofetch). */
  status?: number
  /** HTTP status (H3-style). */
  statusCode?: number
  /** The upstream response body. */
  data?: unknown
}

/**
 * Reads the HTTP status off a `$fetch` rejection.
 *
 * @param err - Whatever `$fetch` threw.
 * @returns The upstream status, or 502 when the request never got an answer.
 */
const upstreamStatus = (err: UpstreamFailure): number => {
  return err?.status ?? err?.statusCode ?? 502
}

/**
 * Stores a token the backend renewed (sliding session) in the browser's `auth_token` cookie.
 *
 * @param event - H3 event of the incoming browser request.
 * @param renewedToken - Value of the renewal header, null or empty when no renewal was offered.
 */
const forwardRenewedToken = (event: H3Event, renewedToken: string | null): void => {
  if (!renewedToken) {
    return
  }
  setCookie(event, SessionCookie.AuthToken, renewedToken, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: authCookieMaxAge(renewedToken), // the token's own exp; session cookie when absent
    path: '/',
  })
}

/**
 * Make an authenticated request to the internal C# backend API.
 *
 * On 401 responses, automatically attempts a token refresh and retries
 * the request once. If refresh fails, throws 401 to the client.
 *
 * @param event - H3 event from the Nuxt server route handler
 * @param endpoint - API path after `/api`, e.g. `/clients/me`
 * @param options - Optional method, body, extra headers; `anonymous` omits the bearer token for
 *   the calls made before a session exists (sign-in, registration, password reset)
 * @returns Parsed JSON response typed as `T`
 * @throws H3Error forwarding any upstream HTTP error status
 */
export const internalApiCall = async <T>(
  event: H3Event,
  endpoint: string,
  options: InternalApiOptions = {}
): Promise<T> => {
  const apiUrl = getApiUrl()
  const token = options.anonymous ? undefined : getCookie(event, SessionCookie.AuthToken)

  /**
   * Builds the headers for one attempt; the language is read once per attempt from the incoming request.
   *
   * @param bearerToken - Access token to send, absent for an anonymous call.
   * @returns The header map.
   */
  const buildHeaders = (bearerToken: string | undefined): Record<string, string> => {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      [LANGUAGE_HEADER]: backendLanguage(event),
      ...options.headers,
    }
    if (bearerToken) {
      headers['Authorization'] = `Bearer ${bearerToken}`
    }
    return headers
  }

  // Bound to a plain `string` first. Passed as a template literal, Nitro tries to match the URL
  // against its own route table and answers with `TypedInternalResponse<...>` rather than `T` --
  // even though this address is the external C# API and matches no Nitro route at all.
  const url: string = `${apiUrl}/api${endpoint}`

  /**
   * Sends the request once with the given token.
   *
   * @param bearerToken - Access token to send, absent for an anonymous call.
   * @returns The parsed response.
   */
  const makeRequest = (bearerToken: string | undefined): Promise<T> =>
    $fetch<T>(url, {
      method: options.method ?? 'GET',
      headers: buildHeaders(bearerToken),
      body: options.body,
      onResponse: ({ response }) => {
        forwardRenewedToken(event, response.headers.get(RENEWED_TOKEN_HEADER))
      },
    })

  try {
    return await makeRequest(token)
  } catch (err: unknown) {
    const failure = err as UpstreamFailure
    const status = upstreamStatus(failure)

    // On 401: attempt token refresh and retry (skip for auth endpoints to avoid loops)
    const isAuthEndpoint = endpoint.startsWith('/auth/')
    if (status === 401 && !isAuthEndpoint && !options.anonymous) {
      const newToken = await tryRefreshToken(event)
      if (newToken) {
        try {
          return await makeRequest(newToken)
        } catch (retryErr: unknown) {
          const retryFailure = retryErr as UpstreamFailure
          throw backendError(upstreamStatus(retryFailure), retryFailure)
        }
      }
    }

    throw backendError(status, failure)
  }
}

/**
 * Set the JWT access token cookie on the response.
 *
 * MaxAge comes from the token's own `exp` claim (a session cookie when it has none).
 * Also sets the `authed` flag cookie (7 days, non-httpOnly) so client-side
 * middleware can check login status without reading the token.
 *
 * @param event - H3 event
 * @param token - JWT access token from the C# API
 */
export const setAuthCookie = (event: H3Event, token: string): void => {
  setCookie(event, SessionCookie.AuthToken, token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: authCookieMaxAge(token), // the token's own exp; session cookie when absent
    path: '/',
  })

  setCookie(event, SessionCookie.Authed, '1', {
    httpOnly: false,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    // Deliberately NOT REFRESH_TOKEN_MAX_AGE. This is only a hint to client-side
    // middleware that a session probably exists; it is not a credential. The comment
    // here used to claim it matched the refresh token's lifetime, which it never did —
    // the SSO issues those for 30 days. Whether the hint should be stretched to match
    // is a separate question from the credential bug, and stretching it silently would
    // make the middleware assert a session for three more weeks than it does today.
    maxAge: AUTHED_FLAG_MAX_AGE,
    path: '/',
  })
}

/**
 * Set the refresh token cookie on the response.
 *
 * httpOnly and Secure in production. 7-day lifetime to match the
 * backend's refresh token expiry.
 *
 * @param event - H3 event
 * @param token - Opaque refresh token from the C# API
 */
export const setRefreshCookie = (event: H3Event, token: string): void => {
  setCookie(event, SessionCookie.RefreshToken, token, {
    httpOnly: true,
    secure: process.env.NODE_ENV === 'production',
    sameSite: 'lax',
    maxAge: REFRESH_TOKEN_MAX_AGE,
    path: '/',
  })
}

/**
 * Clear all auth-related cookies (access token, refresh token, and flag).
 *
 * @param event - H3 event
 */
export const clearAllAuthCookies = (event: H3Event): void => {
  deleteCookie(event, SessionCookie.AuthToken, { path: '/' })
  deleteCookie(event, SessionCookie.RefreshToken, { path: '/' })
  deleteCookie(event, SessionCookie.Authed, { path: '/' })
  deleteCookie(event, SessionCookie.PendingAccount, { path: '/' })
}
