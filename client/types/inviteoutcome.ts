/**
 * How accepting a client invitation ended, as reported by `stores/auth.ts` `acceptInvite`.
 */
export enum InviteOutcome {
  /** The invitation was accepted and the account is linked to the client. */
  Accepted = 'accepted',
  /** Nobody is signed in yet; the invitation itself is fine and the page offers the sign-in step. */
  SignInRequired = 'sign-in-required',
  /** The invitation was refused (expired, already accepted, offline); the reason is in the store. */
  Failed = 'failed'
}
