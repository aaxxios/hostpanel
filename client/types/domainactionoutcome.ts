/**
 * How a domain action in `stores/domains.ts` ended.
 *
 * A closed set the pages branch on, so it is an enum rather than a pair of string literals.
 * The reason for a failure is never carried here; it is the store's own error state.
 */
export enum DomainActionOutcome {
  /** The API accepted the request. */
  Done = 'done',
  /** The request was refused or never reached the API; the reason is in the store. */
  Failed = 'failed'
}
