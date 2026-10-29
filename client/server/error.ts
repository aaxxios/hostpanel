/**
 * Nitro error handler for this app.
 *
 * It exists for one reason: Nitro's built-in handler serialises a thrown `H3Error` as
 * `{ url, statusCode, statusMessage, message, stack }` and **drops `data`**. That is the only
 * field carrying the backend's machine-readable `code`, so without this file the code the C#
 * API sends never reaches the browser and the portal would be back to matching on English
 * prose to tell one refusal from another.
 *
 * The blast radius is kept deliberately small. Only API requests that actually carry a `data`
 * payload are handled here; for everything else (a page render, which Nuxt's handler turns into
 * `error.vue`; dev-mode HTML error pages; the unhandled-error logging) this handler answers
 * nothing, so the next handler in the chain (registered by `nuxt.config.ts`) takes the request.
 *
 * @module server/error
 */

import { defineNitroErrorHandler } from 'nitropack/runtime'
import { send, setResponseHeader, setResponseHeaders, setResponseStatus } from 'h3'

/** The shape `internalApiCall` attaches to `createError({ data })`. */
interface ErrorData {
  /** The backend's SCREAMING_SNAKE error code, or null when it sent none. */
  code?: string | null
  /** The backend's localized summary, kept so a client can fall back to it when `detail` is absent. */
  title?: string | null
  /** Field name to messages of a 400 `VALIDATION_ERROR`, or null. */
  errors?: Record<string, string[]> | null
}

/** An H3 error as it looks once `createError({ data })` has attached a payload. */
interface ErrorWithData {
  /** The payload `internalApiCall` attached, when there is one. */
  data?: ErrorData
}

/**
 * Serialises an API error, preserving the `data` Nitro would otherwise discard.
 *
 * @param error - The thrown H3 error.
 * @param event - The request being answered.
 * @returns The JSON error response, or nothing for everything this file deliberately does not
 *          handle, which leaves the request to the next handler in the chain.
 */
export default defineNitroErrorHandler(async (error, event) => {
  const data = (error as ErrorWithData).data

  // Anything without a code, and anything outside the API surface, is none of this file's
  // business: answering nothing hands it to the handlers after this one, which render those
  // better than a reimplementation would.
  if (!data || typeof data !== 'object' || !event.path?.startsWith('/api/')) {
    return
  }

  const statusCode = error.statusCode || 500
  // No default sentence: a refusal without a `statusMessage` carries a code only, and the client
  // renders its generic title (an invented "Not Found" would be an English BFF sentence).
  const statusMessage = error.statusMessage ?? ''

  setResponseStatus(event, statusCode, statusMessage)

  // The same hardening Nitro's own handler applies. Copied rather than skipped: an error
  // response is still a response, and dropping these on one branch would quietly make the
  // API's failure path weaker than its success path.
  setResponseHeaders(event, {
    'Content-Security-Policy': "script-src 'none'; frame-ancestors 'none';",
    'X-Content-Type-Options': 'nosniff',
    'X-Frame-Options': 'DENY',
    'Referrer-Policy': 'no-referrer',
  })
  setResponseHeader(event, 'Content-Type', 'application/json')

  // Same field names Nitro uses, plus `data`. Keeping the existing four identical matters:
  // roughly twenty pages already read `statusMessage` as the sentence to show, and this
  // change must not move that sentence.
  return send(event, JSON.stringify({
    url: event.path || '',
    statusCode,
    statusMessage,
    message: error.message || statusMessage,
    data,
  }))
})
