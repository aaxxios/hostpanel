/**
 * Reading a failed portal call: the sentence to show, the field messages, and the code to branch on.
 *
 * This is the one place a failure sentence is chosen on the client. There is **no mapping table**:
 * the API answers a refusal with a finished sentence (`detail`) already written in the caller's
 * language (the transport stamps `X-Language`), so a page renders `apiErrorMessage(err)` and is
 * done. The order is field message, then `detail`, then `title`; only when the server said
 * nothing at all does the client speak, with its single "no answer" sentence (`errors.noAnswer`).
 * `err.message` is deliberately never a fallback: on a response with no body `$fetch` composes a
 * message that embeds the address it called.
 *
 * **Codes are the contract.** A page or store may branch on {@link apiErrorCode} and never on the
 * sentence, which is reworded in three languages.
 *
 * @module lib/errorHandler
 */

import type { ApiFailureBody } from '~/types/apifailurebody'
import { translate } from '~/utils/translate'

// The codes live in `constants/` so the Nitro BFF can use them without importing this reader.
export { PortalErrorCode } from '~/constants/portalErrorCode'
export type { PortalErrorCodeValue } from '~/constants/portalErrorCode'

/** Whatever a failed call threw, as far as this module reads it. */
interface ThrownWithBody {
  /** The parsed response body, when the call reached the server. */
  data?: ApiFailureBody
}

/**
 * Digs the response body out of whatever a call threw.
 *
 * @param err - Whatever `apiFetch` / `$fetch` threw; anything that is not an object has no body.
 * @returns The parsed body, or an empty object when there is none (offline, a thrown string).
 */
const failureBody = (err: unknown): ApiFailureBody => {
  const body = (err as ThrownWithBody | null | undefined)?.data
  return body !== null && typeof body === 'object' ? body : {}
}

/**
 * Returns a string only when it holds visible text.
 *
 * @param value - A candidate sentence.
 * @returns The value, or null when it is absent or blank.
 */
const textOrNull = (value: unknown): string | null => {
  return typeof value === 'string' && value.trim() !== '' ? value : null
}

/**
 * Reads the machine-readable code out of a rejected call.
 *
 * Two shapes are checked because a refusal is reshaped once on the way out: a call through a
 * Nitro route carries the backend's `type` under `data.data.code`; a call that reached the C# API
 * directly carries its problem+json body unwrapped (`data.type`).
 *
 * @param err - Whatever the call threw.
 * @returns The code, or `null` when the response carried none, including a request that never
 *          got an answer at all.
 */
export const apiErrorCode = (err: unknown): string | null => {
  const body = failureBody(err)
  // legacy-gate: allow: `data.code` is the BFF's own field (server/utils/api.ts), not the backend's old pair
  return body.data?.code ?? body.type ?? null
}

/**
 * Reads the field-level messages of a 400 `VALIDATION_ERROR`.
 *
 * @param err - Whatever the call threw.
 * @returns Field name (camelCase, as on the wire) to that field's first message; empty when the
 *          refusal was not about fields. A nested field arrives as a dotted path (`address.street`);
 *          the server names each segment in PascalCase, so every segment is lower-cased at its first letter.
 */
export const apiFieldErrors = (err: unknown): Record<string, string> => {
  const body = failureBody(err)
  const raw = body.data?.errors ?? body.errors ?? {}
  const result: Record<string, string> = {}
  for (const [field, messages] of Object.entries(raw)) {
    const first = textOrNull(messages?.[0])
    if (first !== null) {
      result[field.split('.').map(segment => segment.charAt(0).toLowerCase() + segment.slice(1)).join('.')] = first
    }
  }
  return result
}

/**
 * Chooses the sentence to show for a failed call.
 *
 * @param err - Whatever the call threw.
 * @returns The first field message, else the API's `detail` (carried as `statusMessage` through the
 *          BFF), else its `title`, else, with nothing from the server to show, the client's own
 *          "no answer" sentence. Never empty.
 */
export const apiErrorMessage = (err: unknown): string => {
  const [firstField] = Object.values(apiFieldErrors(err))
  if (firstField !== undefined) {
    return firstField
  }
  const body = failureBody(err)
  return textOrNull(body.detail)
    ?? textOrNull(body.statusMessage)
    ?? textOrNull(body.title)
    ?? textOrNull(body.data?.title)
    ?? translate('errors.noAnswer')
}
