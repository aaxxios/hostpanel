/**
 * An order, its detail and one line on it.
 */


/** DTO for order list items in admin view. */
export interface OrderListItem {
  /** Unique order identifier. */
  id: number
  /** Human-readable order number (e.g. "ORD-0001"). */
  orderNumber: string
  /** FK to the owning client. */
  clientId: number
  /** Client display name; null when unresolved. */
  clientName: string | null
  /** Current order status. */
  status: OrderStatus
  /** Payment gateway module name. */
  paymentMethod: string
  /** Total order amount. */
  total: number
  /** ISO 4217 code the order's amounts are in. */
  currency: string
  /** Linked invoice ID, if any. */
  invoiceId: number | null
  /** Number of items in the order. */
  itemCount: number
  /** ISO 8601 creation timestamp. */
  createdAt: string
}

/** DTO for full order details in admin view. */
export interface OrderDetail extends OrderListItem {
  /** Invoice status if invoice exists. */
  invoiceStatus: string | null
  /** Client IP address at checkout. */
  ipAddress: string | null
  /** Admin notes. */
  notes: string | null
  /** Order line items. */
  items: OrderItemDetail[]
}

/** DTO for an individual order item. */
export interface OrderItemDetail {
  /** Item ID. */
  id: number
  /** Product ID. */
  productId: number
  /** Product name snapshot at order time. */
  productName: string
  /** Billing cycle. */
  billingCycle: string
  /** Domain name if applicable. */
  domain: string | null
  /** Hostname if applicable. */
  hostname: string | null
  /** First payment amount. */
  firstPaymentAmount: number
  /** Recurring amount. */
  recurringAmount: number
  /** Item status. */
  status: OrderItemStatus
}

/** Filter and paging of the order list. */
export interface OrderFilter {
  /** Page, 1-based. */
  page: number
  /** Items per page. */
  pageSize: number
  /** Status to keep; null or empty for all. */
  status?: string | null
}

/** Status of an order, exactly as the API sends it. */
export enum OrderStatus {
  Pending = 'Pending',
  Active = 'Active',
  Cancelled = 'Cancelled',
  Fraud = 'Fraud',
}

/** Status of one line on an order, exactly as the API sends it. */
export enum OrderItemStatus {
  Pending = 'Pending',
  Active = 'Active',
  Cancelled = 'Cancelled',
}
