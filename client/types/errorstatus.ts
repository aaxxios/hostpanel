/**
 * HTTP statuses the error page has its own wording and icon for.
 *
 * Any other status falls back to the generic "something went wrong" presentation.
 */
export enum ErrorStatus {
  /** The page does not exist. */
  NotFound = 404,
  /** The visitor is not allowed to see the page. */
  Forbidden = 403,
  /** The server failed. */
  ServerError = 500
}
