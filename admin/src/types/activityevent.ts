/** The kinds of event the admin notification feed can surface. */
export const ActivityEventType = {
  /** A new client signed up. */
  ClientRegistered: 'ClientRegistered',
  /** An invoice passed its due date unpaid. */
  InvoiceOverdue: 'InvoiceOverdue',
  /** A domain is close to its expiry date. */
  DomainExpiring: 'DomainExpiring',
} as const

/** One of the {@link ActivityEventType} values. */
export type ActivityEventType = (typeof ActivityEventType)[keyof typeof ActivityEventType]

/** A single event in the admin notification feed. */
export interface ActivityEvent {
  /** Machine-readable event kind, used to pick an icon/label. */
  type: ActivityEventType
  /** Human-readable summary of the event. */
  message: string
  /** ISO timestamp of when the event happened. */
  occurredAt: string
  /** Primary key of the client, invoice or domain this event is about. */
  entityId: number
}
