/**
 * How saving a contact from the account page ended, as reported by `stores/account.ts`.
 */
export enum ContactSaveOutcome {
  /** The contact was created or updated. */
  Saved = 'saved',
  /** A required name was missing, so no request was made. */
  Invalid = 'invalid',
  /** The API refused or was unreachable; the reason is in the store's `contactError`. */
  Failed = 'failed'
}
