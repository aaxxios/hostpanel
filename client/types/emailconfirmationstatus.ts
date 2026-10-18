/**
 * Where an email-confirmation attempt stands, held by `stores/auth.ts` for the landing page.
 */
export enum EmailConfirmationStatus {
  /** The request has not finished (or has not started). */
  Pending = 'pending',
  /** The address is confirmed. */
  Confirmed = 'confirmed',
  /** The link was empty or the API refused it; the reason is in the store. */
  Failed = 'failed'
}
