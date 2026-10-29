/**
 * The machine-readable codes the external sign-on HTTP util puts in `data.code` of the errors it
 * throws. The BFF writes no sentence; the browser maps the code.
 *
 * @module types/signonerrorcode
 */
export enum SignOnErrorCode {
  /** The sign-on service did not answer within the time allowed. */
  Timeout = 'SIGN_ON_TIMEOUT',
  /** The sign-on service could not be reached, or answered with a server error. */
  Unavailable = 'SIGN_ON_UNAVAILABLE',
  /** The sign-on service refused the request (a 4xx answer). */
  Refused = 'SIGN_ON_REFUSED'
}
