/**
 * Reads one record id out of a route query value.
 *
 * A query value is whatever the address carried: a string, a repeated key (an array) or nothing. A
 * page that builds a request path from it must accept exactly one positive whole number and treat
 * everything else as absent, so `?order=1&order=2` or `?order=../x` never reach a request.
 *
 * @module utils/queryId
 */

/** Matches a positive whole number written in decimal digits. */
const POSITIVE_INTEGER = /^[1-9]\d*$/

/**
 * Narrows a query value to a single record id.
 *
 * @param value - The raw query value.
 * @returns The id as a string, or `undefined` when the value is missing, repeated or not a positive whole number.
 */
export const queryId = (value: unknown): string | undefined => {
  if (typeof value === 'number') {
    return Number.isSafeInteger(value) && value > 0 ? String(value) : undefined
  }
  return typeof value === 'string' && POSITIVE_INTEGER.test(value) ? value : undefined
}
