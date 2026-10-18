/**
 * Filter tabs shared by the client list pages (domains, invoices, tickets).
 *
 * Each page maps the member to its own predicate; `All` always means "no filter".
 */
export enum ListFilter {
  /** Every row. */
  All = 'all',
  /** Rows still needing attention: active domains, unpaid invoices, open tickets. */
  Current = 'current',
  /** Rows that are finished: expired domains, paid invoices, closed tickets. */
  Finished = 'finished'
}
