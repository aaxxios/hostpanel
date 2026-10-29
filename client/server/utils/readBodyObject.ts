/**
 * Request-body guard for the Nitro routes.
 *
 * `readBody` hands back whatever the browser sent: nothing, a string, a number, an array. The
 * routes forward the body to the backend as a JSON object, so anything else is refused here with
 * a coded 400 and no sentence (the BFF writes none).
 *
 * @module server/utils/readBodyObject
 */

import type { H3Event } from 'h3'

/** The `data.code` of the 400 a body that is not a JSON object answers with. */
export const BAD_BODY = 'BAD_BODY'

/**
 * Reads the request body and refuses the request when it is not a JSON object.
 *
 * @param event - H3 event of the incoming request.
 * @returns The body as an object.
 * @throws H3Error 400 carrying `data.code = BAD_BODY` and no `statusMessage`.
 */
export const readBodyObject = async (event: H3Event): Promise<Record<string, unknown>> => {
  const body: unknown = await readBody(event)
  if (body === null || typeof body !== 'object' || Array.isArray(body)) {
    throw createError({ statusCode: 400, data: { code: BAD_BODY } })
  }
  return body as Record<string, unknown>
}
