/**
 * Query-string guard for the Nitro routes.
 *
 * `getQuery` returns a string, an array (the key repeated), or nothing. A route that reads a
 * single value must not be handed an array posing as one, so a repeated key is a coded 400
 * and no sentence (the BFF writes none).
 *
 * @module server/utils/readQueryString
 */

/** The `data.code` of the 400 a query value that is not a single string answers with. */
export const BAD_QUERY = 'BAD_QUERY'

/**
 * Reads one query value as a string.
 *
 * @param value - The value from `getQuery`.
 * @returns The string, or undefined when the key is absent.
 * @throws H3Error 400 carrying `data.code = BAD_QUERY` and no `statusMessage` when the value is not a single string.
 */
export const readQueryString = (value: unknown): string | undefined => {
  if (value === undefined) {
    return undefined
  }
  if (typeof value !== 'string') {
    throw createError({ statusCode: 400, data: { code: BAD_QUERY } })
  }
  return value
}
