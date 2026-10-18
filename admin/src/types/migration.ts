/**
 * Shapes of the migration endpoints of the admin API: jobs, their progress, their import log and
 * the options that start one.
 */

/** Import counters of one entity group. */
export interface MigrationEntityProgress {
  /** Rows written. */
  imported: number
  /** Rows skipped because they already existed. */
  skipped: number
  /** Rows the source reported for this group. */
  total: number
  /** True once the group has been fully processed. */
  done: boolean
}

/** Progress of every entity group of a job. */
export interface MigrationProgress {
  /** Clients. */
  clients: MigrationEntityProgress
  /** Invoices. */
  invoices: MigrationEntityProgress
  /** Services. */
  services: MigrationEntityProgress
  /** Domains. */
  domains: MigrationEntityProgress
  /** Tickets. */
  tickets: MigrationEntityProgress
  /** Products. */
  products: MigrationEntityProgress
  /** Orders. */
  orders: MigrationEntityProgress
  /** Transactions. */
  transactions: MigrationEntityProgress
  /** Quotes. */
  quotes: MigrationEntityProgress
  /** Knowledgebase articles. */
  knowledgebase: MigrationEntityProgress
  /** Contacts. */
  contacts: MigrationEntityProgress
  /** Ticket replies. */
  ticketReplies: MigrationEntityProgress
  /** Announcements. */
  announcements: MigrationEntityProgress
  /** Downloads. */
  downloads: MigrationEntityProgress
  /** Network issues. */
  networkIssues: MigrationEntityProgress
}

/** Which entity groups a job exports. */
export interface MigrationEntitySelection {
  /** Export clients. */
  clients: boolean
  /** Export invoices. */
  invoices: boolean
  /** Export services. */
  services: boolean
  /** Export domains. */
  domains: boolean
  /** Export tickets. */
  tickets: boolean
  /** Export products. */
  products: boolean
  /** Export orders. */
  orders: boolean
  /** Export transactions. */
  transactions: boolean
  /** Export quotes. */
  quotes: boolean
  /** Export knowledgebase articles. */
  knowledgebase: boolean
  /** Export contacts. */
  contacts: boolean
  /** Export ticket replies. */
  ticketReplies: boolean
  /** Export announcements. */
  announcements: boolean
  /** Export downloads. */
  downloads: boolean
  /** Export network issues. */
  networkIssues: boolean
}

/** One line of a job's import log. */
export interface MigrationLogEntry {
  /** Log row id. */
  id: number
  /** The entity group the row belongs to. */
  entityType: MigrationEntityType
  /** Identifier of the source row. */
  identifier: string
  /** What happened to the row. */
  action: MigrationLogAction
  /** Why the row was skipped or failed, null otherwise. */
  reason: string | null
  /** When the row was logged. */
  createdAt: string
}

/** One page of a job's import log. */
export interface MigrationLogPage {
  /** The rows on this page. */
  items: MigrationLogEntry[]
  /** Rows matching the filter across all pages. */
  totalCount: number
  /** One-based page number. */
  page: number
  /** Rows per page. */
  pageSize: number
}

/** A migration job as the API returns it. */
export interface MigrationJob {
  /** Job id. */
  id: number
  /** Public key the source plugin authenticates with. */
  key: string
  /** Address of the source system. */
  sourceUrl: string
  /** Lifecycle state. */
  status: MigrationStatus
  /** Label the operator gave the job, null when none. */
  label: string | null
  /** Why the job failed, null otherwise. */
  errorMessage: string | null
  /** Which entity groups the job exports. */
  entitySelection: MigrationEntitySelection
  /** Per-group counters. */
  progress: MigrationProgress
  /** Overall completion, 0 to 100. */
  overallPercent: number
  /** True when the source plugin has connected. */
  pluginConnected: boolean
  /** Last time the source plugin pinged, null when never. */
  lastPingAt: string | null
  /** When the job was created. */
  createdAt: string
  /** When the job finished, null while running. */
  completedAt: string | null
}

/** Options that create a migration job; every export flag defaults to true. */
export interface CreateMigrationJobOptions {
  /** Label for the job. */
  label?: string
  /** Address of the source system. */
  sourceUrl: string
  /** Export clients. */
  exportClients?: boolean
  /** Export invoices. */
  exportInvoices?: boolean
  /** Export services. */
  exportServices?: boolean
  /** Export domains. */
  exportDomains?: boolean
  /** Export tickets. */
  exportTickets?: boolean
  /** Export products. */
  exportProducts?: boolean
  /** Export orders. */
  exportOrders?: boolean
  /** Export transactions. */
  exportTransactions?: boolean
  /** Export quotes. */
  exportQuotes?: boolean
  /** Export knowledgebase articles. */
  exportKnowledgebase?: boolean
  /** Export contacts. */
  exportContacts?: boolean
  /** Export ticket replies. */
  exportTicketReplies?: boolean
  /** Export announcements. */
  exportAnnouncements?: boolean
  /** Export downloads. */
  exportDownloads?: boolean
  /** Export network issues. */
  exportNetworkIssues?: boolean
}

/** Body of `POST /admin/migrations`: the `CreateMigrationJobCommand` as the API binds it. */
export interface CreateMigrationJobPayload {
  /** Label for the job, null when none. */
  label: string | null
  /** Address of the source system. */
  sourceUrl: string
  /** Which entity groups the job exports. */
  selection: MigrationEntitySelection
}

/** Filter and paging of a job's import log. */
export interface MigrationLogQuery {
  /** Keep only rows with this action. */
  action?: string
  /** Keep only rows of this entity group. */
  entityType?: string
  /** One-based page number, 1 by default. */
  page?: number
  /** Rows per page, 50 by default. */
  pageSize?: number
}

/** Closed set of lifecycle states of a migration job. */
export const MigrationStatus = {
  /** Created; waiting for the source plugin and for the import to start. */
  Pending: 'Pending',
  /** The import is running. */
  InProgress: 'InProgress',
  /** The import finished. */
  Completed: 'Completed',
  /** The import stopped with an error. */
  Failed: 'Failed',
} as const

/** Closed set of outcomes the import log records for a row. */
export const MigrationLogAction = {
  /** The row was written. */
  Imported: 'Imported',
  /** The row already existed and was skipped. */
  Skipped: 'Skipped',
  /** The row could not be written. */
  Failed: 'Failed',
} as const

/** Keys of the entity groups a job can export, in display order. */
export const MIGRATION_ENTITY_KEYS = [
  'clients', 'invoices', 'services', 'domains', 'tickets', 'products', 'orders', 'transactions',
  'quotes', 'knowledgebase', 'contacts', 'ticketReplies', 'announcements', 'downloads', 'networkIssues',
] as const

/** One entity group key. */
export type MigrationEntityKey = (typeof MIGRATION_ENTITY_KEYS)[number]

/** The export flags of a job, as a form holds them before the label and address are added. */
export interface MigrationExportFlags {
  /** Export clients. */
  exportClients: boolean
  /** Export invoices. */
  exportInvoices: boolean
  /** Export services. */
  exportServices: boolean
  /** Export domains. */
  exportDomains: boolean
  /** Export tickets. */
  exportTickets: boolean
  /** Export products. */
  exportProducts: boolean
  /** Export orders. */
  exportOrders: boolean
  /** Export transactions. */
  exportTransactions: boolean
  /** Export quotes. */
  exportQuotes: boolean
  /** Export knowledgebase articles. */
  exportKnowledgebase: boolean
  /** Export contacts. */
  exportContacts: boolean
  /** Export ticket replies. */
  exportTicketReplies: boolean
  /** Export announcements. */
  exportAnnouncements: boolean
  /** Export downloads. */
  exportDownloads: boolean
  /** Export network issues. */
  exportNetworkIssues: boolean
}

/** One of the {@link MigrationStatus} values. */
export type MigrationStatus = (typeof MigrationStatus)[keyof typeof MigrationStatus]

/** One of the {@link MigrationLogAction} values. */
export type MigrationLogAction = (typeof MigrationLogAction)[keyof typeof MigrationLogAction]

/** Closed set of entity groups the import log names a row under, as the API sends them. */
export const MigrationEntityType = {
  Clients: 'Clients',
  Invoices: 'Invoices',
  Services: 'Services',
  Domains: 'Domains',
  Tickets: 'Tickets',
  Products: 'Products',
  Orders: 'Orders',
  Transactions: 'Transactions',
  Quotes: 'Quotes',
  Knowledgebase: 'Knowledgebase',
  Contacts: 'Contacts',
  TicketReplies: 'TicketReplies',
  Announcements: 'Announcements',
  Downloads: 'Downloads',
  NetworkIssues: 'NetworkIssues',
} as const

/** One of the {@link MigrationEntityType} values. */
export type MigrationEntityType = (typeof MigrationEntityType)[keyof typeof MigrationEntityType]
