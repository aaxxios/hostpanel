/**
 * How a store action that talks to the API ended.
 *
 * A closed set the page branches on, so it is an enum rather than a pair of string literals.
 * The refusal sentence is never carried here; it lives in the owning store's error state.
 */
export enum ActionOutcome {
  /** The API accepted the request. */
  Succeeded = 'succeeded',
  /** The request was refused or never reached the API; the reason is in the store's error state. */
  Failed = 'failed'
}
