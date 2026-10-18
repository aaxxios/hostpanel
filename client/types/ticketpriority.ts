/**
 * Priorities a client can give a new support ticket.
 *
 * The values cross the wire as the ticket's `priority` field, so they are the API's wording.
 */
export enum TicketPriority {
  /** Not urgent. */
  Low = 'Low',
  /** The default. */
  Medium = 'Medium',
  /** Needs attention soon. */
  High = 'High'
}
