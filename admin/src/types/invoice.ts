/**
 * An invoice, its lines, its payments and the status rollup the lists show.
 * 
 * Kept together because none of them is meaningful alone: a line belongs to an invoice, a
 * transaction settles one, and the count is what a screen renders above a list of them.
 */


/** Single line item on an invoice. */
export interface InvoiceItem {
  /** Unique line item identifier. */
  id: number
  /** Human-readable charge description. */
  description: string
  /** Price per unit. */
  unitPrice: number
  /** Number of units. */
  quantity: number
  /** Line total (unitPrice x quantity). */
  amount: number

  /**
   * BUG — read by `InvoiceDetailView`'s "Taxed" column, but `InvoiceItemDto` has no such
   * field, so the column has always rendered "No" for every line regardless of tax.
   *
   * Declared optional so the type states what actually arrives (`undefined`) instead of
   * promising a boolean. Whether tax is per-line or invoice-wide (`Invoice.taxRate`) is a
   * product decision; nothing here can infer it.
   */
  taxed?: boolean
}

/** Payment, refund, or credit transaction recorded against an invoice. */
export interface InvoiceTransaction {
  /** Unique transaction identifier. */
  id: number
  /** Transaction date (ISO 8601). */
  date: string
  /** Payment gateway used. */
  gateway: string
  /** Gateway transaction reference. */
  transactionId: string
  /** Transaction amount. */
  amount: number
  /** Transaction fees charged by the gateway. */
  fees: number
  /** Transaction type: Payment, Refund, or Credit. */
  type: string
  /** Optional admin notes. */
  notes?: string
}

/** Represents an invoice. */
export interface Invoice {
  /** Unique invoice identifier. */
  id: number
  /** Associated client identifier. */
  clientId: number
  /** Invoice status (Draft, Unpaid, Paid, Overdue, Cancelled, Refunded, Collections, PaymentPending). */
  status: string
  /** ISO 8601 due date. */
  dueDate: string
  /** ISO 8601 creation timestamp. */
  createdAt: string
  /** Total amount due. */
  total: number
  /** Optional admin notes. */
  notes?: string
  /** ISO 8601 invoice date. */
  invoiceDate: string
  /** Payment method label. */
  paymentMethod?: string
  /** Tax rate percentage. */
  taxRate: number
  /** Computed tax amount. */
  tax: number
  /** Sub-total before tax and credit. */
  subTotal: number
  /** Credit applied to the invoice. */
  credit: number
  /** ISO 8601 payment timestamp, if paid. */
  paidAt?: string
  /** Gateway transaction reference, if paid. */
  gatewayTransactionId?: string
  /** Display name of the owning client; null when unresolved. */
  clientName: string | null
  /** Line items on the invoice. */
  items: InvoiceItem[]
  /** Payment/refund/credit transactions. */
  transactions: InvoiceTransaction[]

  /**
   * BUG — read by `InvoiceDetailView` (the "Payment Method" summary field and the payment
   * panel), but `InvoiceDto` sends no `gateway`. Both call sites therefore show their
   * fallback — '' and 'Not set' — on every invoice, paid or not.
   *
   * The API's nearest equivalents are {@link Invoice.paymentMethod} and
   * {@link Invoice.gatewayTransactionId}. Switching to one of those changes what the screen
   * displays, so it is left as-is and recorded here.
   */
  gateway?: string

  /**
   * BUG — read by `InvoicesListView`'s "Last Updated" column as
   * `invoice.updatedAt || invoice.createdAt`, but `InvoiceListItemDto` has no `updatedAt`.
   * The column has always shown the creation date.
   */
  updatedAt?: string
}

/** Invoice count and total for a single status. */
export interface InvoiceStatusCount {
  /** Number of invoices in this status. */
  count: number
  /** Sum of totals for invoices in this status. */
  total: number
}

/** Filters the invoice list endpoints accept; an empty value is left off the request. */
export interface InvoiceFilters {
  /** Status filter, e.g. `Unpaid`. */
  status?: string
  /** Start date filter (ISO 8601). */
  from?: string
  /** End date filter (ISO 8601). */
  to?: string
}

/** One line of a new invoice. */
export interface InvoiceItemInput {
  /** Human-readable charge description. */
  description: string
  /** Price per unit. */
  unitPrice: number
  /** Number of units. */
  quantity: number
}

/** Body of `POST /billing`. */
export interface CreateInvoicePayload {
  /** The client to invoice. */
  clientId: number
  /** ISO 8601 due date. */
  dueDate: string
  /** Line items to include. */
  items: InvoiceItemInput[]
  /** True to create as draft, false to create unpaid. */
  isDraft: boolean
}

/**
 * One entry of `PUT /billing/{id}/items`, mirroring the API UpdateItemEntryRequest:
 * `id` is null for a new line, and `isDeleted` removes an existing one.
 */
export interface InvoiceItemUpdate extends InvoiceItemInput {
  /** Existing line item id; null for a new line. */
  id: number | null
  /** When true, the line is removed from the invoice. */
  isDeleted: boolean
}

/** Body of `PUT /billing/{id}/options`. */
export interface InvoiceOptionsPayload {
  /** ISO 8601 invoice date. */
  invoiceDate: string
  /** ISO 8601 due date. */
  dueDate: string
  /** Payment method label. */
  paymentMethod: string
  /** Tax rate percentage. */
  taxRate: number
}

/** Body of `POST /billing/{id}/payment`. */
export interface InvoicePaymentPayload {
  /** ISO 8601 payment date. */
  date: string
  /** Payment gateway used. */
  gateway: string
  /** Gateway transaction reference. */
  transactionId: string
  /** Amount paid. */
  amount: number
  /** Gateway fees, when any. */
  fees?: number
  /** Admin notes, when any. */
  notes?: string
}

/** Body of `POST /billing/{id}/refund`. */
export interface InvoiceRefundPayload {
  /** Reference of the payment being refunded. */
  transactionId: string
  /** Amount to refund. */
  amount: number
  /** Kind of refund the API understands. */
  refundType: string
  /** Gateway the refund goes through. */
  gateway: string
  /** Reference of the refund itself, when the gateway issued one. */
  refundTransactionId?: string
  /** Admin notes, when any. */
  notes?: string
}

/** Identifier the API answers a create or duplicate with. */
export interface CreatedId {
  /** The new record id. */
  id: number
}

/** Status of an invoice, exactly as the API sends it. */
export enum InvoiceStatus {
  Draft = 'Draft',
  Unpaid = 'Unpaid',
  Paid = 'Paid',
  Overdue = 'Overdue',
  Cancelled = 'Cancelled',
  Refunded = 'Refunded',
  Collections = 'Collections',
  PaymentPending = 'PaymentPending',
}

/** Kinds of transaction recorded against an invoice. */
export enum InvoiceTransactionType {
  Payment = 'Payment',
  Refund = 'Refund',
  Credit = 'Credit',
}

/** Refund kinds the API understands. */
export enum InvoiceRefundType {
  Gateway = 'Gateway',
  Manual = 'Manual',
  CreditBalance = 'CreditBalance',
}

/** Actions `POST /billing/bulk` applies to several invoices at once. */
export enum InvoiceBulkAction {
  MarkPaid = 'MarkPaid',
  MarkUnpaid = 'MarkUnpaid',
  MarkCancelled = 'MarkCancelled',
  Duplicate = 'Duplicate',
  Delete = 'Delete',
}

/** Closed set of ways an invoice detail screen is opened (the route action). */
export const InvoiceOpenAction = {
  /** Read-only view. */
  View: 'view',
  /** Editable. */
  Edit: 'edit',
} as const

/** One of the {@link InvoiceOpenAction} values. */
export type InvoiceOpenAction = (typeof InvoiceOpenAction)[keyof typeof InvoiceOpenAction]

/** Tabs of the invoice detail screen. */
export enum InvoiceTab {
  Summary = 'summary',
  Payment = 'payment',
  Options = 'options',
  Credit = 'credit',
  Refund = 'refund',
  Notes = 'notes',
}
