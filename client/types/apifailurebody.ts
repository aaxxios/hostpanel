/**
 * What a rejected portal call carries, as `lib/errorHandler.ts` reads it.
 *
 * A browser call fails in one of two shapes, because a refusal is reshaped once on its way out of
 * the Nitro BFF (`server/error.ts`): the sentence moves into `statusMessage` and the backend's
 * code, title and field errors move into `data`. A call that reached the C# API directly still
 * carries its problem+json body unwrapped. Both are modelled here so the reader is one function.
 *
 * @module types/apifailurebody
 */

import type { ApiFailureData } from '~/types/apifailuredata'

/** The response body of a rejected call: the BFF envelope and/or a problem+json body. */
export interface ApiFailureBody {
  /** BFF envelope: the backend's `detail`, forwarded as the HTTP status message. */
  statusMessage?: string
  /** BFF envelope: the code, title and field errors. */
  data?: ApiFailureData
  /** Problem+json: the error code. */
  type?: string
  /** Problem+json: the localized summary. */
  title?: string
  /** Problem+json: the localized sentence. */
  detail?: string
  /** Problem+json: field name to messages. */
  errors?: Record<string, string[]>
}
