/**
 * How a password sign-in attempt ended, as reported by `stores/auth.ts` `signIn`.
 *
 * A closed set the page branches on, so it is an enum rather than a pair of string literals.
 */
export enum SignInOutcome {
  /** The credentials were accepted and the account is loaded. */
  SignedIn = 'signed-in',
  /** The account has a second factor on; the sign-in is not finished. */
  TwoFactorRequired = 'two-factor-required',
  /** The sign-in was refused or never reached the API; the reason is in the store's `error`. */
  Failed = 'failed'
}
