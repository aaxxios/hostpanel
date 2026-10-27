/**
 * C# backend API utility -- server-side only
 *
 * All backend API calls go through `internalApiCall`. It:
 * - Reads `apiUrl` from runtimeConfig (set via API_URL env var)
 * - Attaches the access token of the server-side session (`server/utils/portal-session.ts`); the
 *   browser holds only the opaque session cookie, never a token
 * - Forwards the browser's `X-Language` on every backend call (the retry after a refresh included)
 *   so the backend answers its sentences in the reader's language
 * - Sliding session: when the backend answers with a renewed access token
 *   (`X-Renewed-Access-Token`), stores it in the session so it outlives the 15-minute token for as
 *   long as the person keeps working
 * - On 401: refreshes the session's token with its refresh token and retries the request once
 * - Throws appropriate H3 errors on failure; the sentence is always the backend's own `detail`
 *
 * @module server/utils/api
 */

import type { H3Event } from 'h3'
import { LANGUAGE_HEADER, resolveLanguage } from '~/utils/transportLanguage'
import { CSRF_HEADER, CSRF_HEADER_VALUE } from '~/constants/csrfHeader'
import { refreshSession, resolveAccessToken, updateAccessToken } from '~/server/utils/portal-session'

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
 * Reads the backend's refusal body. A file request reads every answer as bytes, refusals
 * included, so a binary body is decoded as the JSON it is.
 *
 * @param data - The `$fetch` rejection's `data`.
 * @returns The refusal body, or undefined when there is none or it is not JSON.
 */
const errorBody = (data: unknown): BackendErrorBody | undefined => {
  if (data instanceof ArrayBuffer || ArrayBuffer.isView(data)) {
    try {
      return JSON.parse(Buffer.from(data as ArrayBuffer).toString('utf8')) as BackendErrorBody
    } catch {
      return undefined
    }
  }
  return data as BackendErrorBody | undefined
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
  const data = errorBody(err?.data)

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
 * Builds the headers of one backend call; the language is read from the incoming request.
 *
 * @param event - H3 event of the incoming browser request.
 * @param options - The call's options; extra headers are merged in last.
 * @param bearerToken - Access token to send, absent for an anonymous call.
 * @returns The header map.
 */
const backendHeaders = (event: H3Event, options: InternalApiOptions, bearerToken: string | undefined): Record<string, string> => {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    [LANGUAGE_HEADER]: backendLanguage(event),
    // The backend's SSO build refuses an anonymous state-changing call without it (a bearer call is exempt).
    [CSRF_HEADER]: CSRF_HEADER_VALUE,
    ...options.headers,
  }
  if (bearerToken) {
    headers['Authorization'] = `Bearer ${bearerToken}`
  }
  return headers
}

/** Watches one backend answer for a renewed access token; passed to `$fetch` as `onResponse`. */
type RenewalWatcher = (context: { response: Response }) => void

/**
 * Runs one backend call with the session's bearer token: a renewed access token the backend
 * offers is stored in the session, a 401 refreshes the token and retries once, and any failure
 * becomes the H3 error the route rethrows to the browser.
 *
 * @param event - H3 event of the incoming browser request.
 * @param endpoint - API path after `/api`; auth endpoints are never retried, to avoid loops.
 * @param options - The call's options; `anonymous` sends no token and never refreshes.
 * @param send - Sends the request once with the given token, passing `watchRenewal` as `onResponse`.
 * @returns Whatever `send` resolves with.
 * @throws H3Error forwarding any upstream HTTP error status
 */
const withSession = async <T>(
  event: H3Event,
  endpoint: string,
  options: InternalApiOptions,
  send: (bearerToken: string | undefined, watchRenewal: RenewalWatcher) => Promise<T>
): Promise<T> => {
  const token = options.anonymous ? undefined : await resolveAccessToken(event)

  /**
   * Sends one attempt and keeps the renewed access token the backend offered on it, if any.
   *
   * @param bearerToken - Access token to send, absent for an anonymous call.
   * @returns Whatever `send` resolves with.
   */
  const attempt = async (bearerToken: string | undefined): Promise<T> => {
    /** The renewed access token the backend offered on this attempt, if any. */
    const renewal: { token: string | null } = { token: null }
    try {
      return await send(bearerToken, ({ response }) => {
        renewal.token = response.headers.get(RENEWED_TOKEN_HEADER)
      })
    } finally {
      // Only a bearer call belongs to a session; an anonymous call has nothing to renew.
      if (renewal.token && bearerToken) {
        await updateAccessToken(event, renewal.token)
      }
    }
  }

  try {
    return await attempt(token)
  } catch (err: unknown) {
    const failure = err as UpstreamFailure
    const status = upstreamStatus(failure)

    // On 401: attempt token refresh and retry (skip for auth endpoints to avoid loops)
    const isAuthEndpoint = endpoint.startsWith('/auth/')
    if (status === 401 && !isAuthEndpoint && !options.anonymous) {
      const newToken = await refreshSession(event)
      if (newToken) {
        try {
          return await attempt(newToken)
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
 * The backend address for an endpoint.
 *
 * @param endpoint - API path after `/api`, e.g. `/clients/me`.
 * @returns The absolute URL.
 */
const backendUrl = (endpoint: string): string => {
  // Bound to a plain `string` first. Passed as a template literal, Nitro tries to match the URL
  // against its own route table and answers with `TypedInternalResponse<...>` rather than `T` --
  // even though this address is the external C# API and matches no Nitro route at all.
  const url: string = `${getApiUrl()}/api${endpoint}`
  return url
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
export const internalApiCall = <T>(
  event: H3Event,
  endpoint: string,
  options: InternalApiOptions = {}
): Promise<T> => {
  const url = backendUrl(endpoint)

  return withSession(event, endpoint, options, (bearerToken, watchRenewal) =>
    $fetch<T>(url, {
      method: options.method ?? 'GET',
      headers: backendHeaders(event, options, bearerToken),
      body: options.body,
      onResponse: watchRenewal,
    })
  )
}

/** A file the backend answered with, ready for a route to hand to the browser. */
export interface BinaryResponse {
  /** The file's bytes. A Buffer, because H3 serialises a bare ArrayBuffer as `{}`. */
  body: Buffer
  /** The backend's `Content-Type`. */
  contentType: string
  /** The backend's `Content-Disposition` (file name and attachment), or null when it sent none. */
  contentDisposition: string | null
  /** The backend's `Cache-Control`, or null when it sent none. */
  cacheControl: string | null
}

/**
 * Makes an authenticated GET to the backend for a file rather than JSON, with the same token
 * refresh and error forwarding as `internalApiCall`.
 *
 * @param event - H3 event from the Nuxt server route handler
 * @param endpoint - API path after `/api`, e.g. `/me/invoices/7/pdf`
 * @returns The file's bytes and the headers a download needs
 * @throws H3Error forwarding any upstream HTTP error status, with the backend's code and sentence
 */
export const internalApiBinaryCall = (event: H3Event, endpoint: string): Promise<BinaryResponse> => {
  const url = backendUrl(endpoint)

  return withSession(event, endpoint, {}, async (bearerToken, watchRenewal) => {
    const response = await $fetch.raw<ArrayBuffer>(url, {
      method: 'GET',
      headers: backendHeaders(event, {}, bearerToken),
      responseType: 'arrayBuffer',
      onResponse: watchRenewal,
    })
    return {
      body: Buffer.from(response._data ?? new ArrayBuffer(0)),
      contentType: response.headers.get('content-type') ?? 'application/octet-stream',
      contentDisposition: response.headers.get('content-disposition'),
      cacheControl: response.headers.get('cache-control'),
    }
  })
}
