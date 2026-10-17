/** Reporting period of the support overview, as the API names it. */
export enum SupportOverviewPeriod {
  /** The current day. */
  Today = 'today',
  /** The previous day. */
  Yesterday = 'yesterday',
  /** The last seven days. */
  Last7Days = 'last7days',
  /** The last thirty days. */
  Last30Days = 'last30days',
}

/** Support overview dashboard statistics. */
export interface SupportOverviewStats {
  /** Number of new tickets in the period. */
  newTickets: number
  /** Number of client replies in the period. */
  clientReplies: number
  /** Number of staff replies in the period. */
  staffReplies: number
  /** Number of tickets with no staff reply in the period. */
  ticketsWithoutReply: number
  /** Average time to first staff response, or null when no data. */
  averageFirstResponse: string | null
  /** Array of 24 integers representing ticket counts per hour (0-23). */
  ticketsByHour: number[]
}
