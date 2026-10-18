/**
 * How a one-shot form submission ended, as reported by the actions in `stores/support.ts`.
 *
 * A closed set the page branches on, so it is an enum rather than a boolean or string literals.
 */
export enum SubmitOutcome {
  /** The API accepted the submission. */
  Sent = 'sent',
  /** The submission was refused or never reached the API; the reason is in the store's error. */
  Failed = 'failed'
}
