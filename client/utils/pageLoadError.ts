/**
 * The error a public detail page raises when its record could not be read.
 *
 * A crawler takes the status literally: a 404 drops the page from the index, a 5xx keeps it and
 * tries again later. So the page answers 404 only when the backend said the record does not exist
 * (or is not published), and passes any other failure on with the status it came with, a request
 * that got no answer at all being a 502. Fatal, so the server render answers with that status and
 * the error page offers a retry.
 *
 * @module utils/pageLoadError
 */

/** The status of a request that got no usable answer: the backend or the network was unreachable. */
const NO_ANSWER_STATUS = 502

/** The status of a record the backend does not have. */
const NOT_FOUND_STATUS = 404

/**
 * The status the BFF answers when the id in the address is not a valid id at all (`/knowledgebase/x`).
 * For a page that is the same as a missing record: no such page exists, so it is reported as a 404.
 */
const MALFORMED_ID_STATUS = 400

/** The lowest HTTP status that is a failure. */
const FIRST_FAILURE_STATUS = 400

/** The highest HTTP status there is. */
const LAST_STATUS = 599

/** The parts of a thrown error that carry an HTTP status, under the names the transports use. */
interface ThrownStatus {
  /** H3-style status. */
  statusCode?: number
  /** ofetch-style status. */
  status?: number
}

/**
 * Reads the HTTP status of a failed read.
 *
 * @param err - Whatever the read threw.
 * @returns The failure status the backend answered with, or 502 when there was none (network error, offline, a thrown string).
 */
export const failureStatus = (err: unknown): number => {
  const thrown = err as ThrownStatus | null | undefined
  const status = thrown?.statusCode ?? thrown?.status
  return typeof status === 'number' && status >= FIRST_FAILURE_STATUS && status <= LAST_STATUS ? status : NO_ANSWER_STATUS
}

/**
 * Builds the error for a detail page whose record is missing.
 *
 * @param status - The failure status of the read, or `null` when the read succeeded and returned nothing.
 * @param notFoundMessage - The status message of the 404, e.g. `Article not found`.
 * @returns A 404 for a missing record or a malformed id, otherwise a fatal error with the upstream status.
 */
export const pageLoadError = (status: number | null, notFoundMessage: string) => {
  if (status === null || status === NOT_FOUND_STATUS || status === MALFORMED_ID_STATUS) {
    return createError({ statusCode: NOT_FOUND_STATUS, statusMessage: notFoundMessage })
  }
  return createError({ statusCode: status, statusMessage: 'The content could not be loaded', fatal: true })
}
