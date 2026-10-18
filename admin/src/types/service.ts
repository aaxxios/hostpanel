/**
 * A provisioned service in its three depths, plus a pending cancellation against one.
 */


/** Represents a hosted service (hosting, VPS, etc.). */
export interface Service {
  /** Unique service identifier. */
  id: number
  /** Associated client identifier. */
  clientId: number
  /** Product name (e.g. Shared Hosting, VPS). */
  productName: string
  /** Primary domain assigned to the service. */
  domain: string
  /** Service status (active, suspended, cancelled, pending). */
  status: string
  /** ISO 8601 next due date for renewal. */
  nextDueDate: string
  /** ISO 8601 creation timestamp. */
  createdAt: string
}

/** Enriched service DTO for admin service list views. */
export interface ServiceListItem {
  /** Unique service identifier. */
  id: number
  /** Associated client identifier. */
  clientId: number
  /** Full name of the owning client. */
  clientName: string
  /** Product display name. */
  productName: string
  /** Linked domain name, or null if no domain is linked. */
  domain: string | null
  /** Resolved price based on billing cycle. */
  price: number
  /** ISO 4217 currency code (e.g. "USD"). */
  priceCurrency: string
  /** Billing cycle: "monthly" or "annual". */
  billingCycle: string
  /** Current lifecycle status (Active, Pending, Suspended, Terminated). */
  status: string
  /** ISO 8601 next renewal date, or null if not yet active. */
  nextDueDate: string | null
}

/** Full service detail returned by GET /api/services/{id}. */
export interface ServiceDetail {
  /** Unique service identifier. */
  id: number
  /** Associated client identifier. */
  clientId: number
  /** Full name of the owning client. */
  clientName: string
  /** Product identifier. */
  productId: number
  /** Product display name. */
  productName: string
  /** Linked domain name, if any. */
  domain?: string
  /** Dedicated IP address, if assigned. */
  dedicatedIp?: string
  /** Control panel or service username. */
  username?: string
  /** Control panel or service password. */
  password?: string
  /** Quantity of units purchased. */
  quantity: number
  /** Amount charged for the first payment. */
  firstPaymentAmount: number
  /** Recurring charge per billing cycle. */
  recurringAmount: number
  /** Payment method identifier or label. */
  paymentMethod?: string
  /** Applied promotion code, if any. */
  promotionCode?: string
  /** External subscription ID (e.g. Stripe subscription). */
  subscriptionId?: string
  /** Billing cycle: "monthly" or "annual". */
  billingCycle: string
  /** Current lifecycle status (Active, Pending, Suspended, Terminated). */
  status: string
  /** External provisioning reference (e.g. cPanel account ID). */
  provisioningRef?: string
  /** ISO 8601 next renewal date, if applicable. */
  nextDueDate?: string
  /** ISO 8601 creation timestamp. */
  createdAt: string
  /** ISO 8601 termination timestamp, if terminated. */
  terminatedAt?: string
  /** Whether automatic suspension is overridden. */
  overrideAutoSuspend: boolean
  /** ISO 8601 date until which the service is suspended. */
  suspendUntil?: string
  /** Whether the service auto-terminates at end of billing cycle. */
  autoTerminateEndOfCycle: boolean
  /** Reason for auto-termination, if applicable. */
  autoTerminateReason?: string
  /** Internal admin notes. */
  adminNotes?: string
  /** FK to the assigned server, if any. */
  serverId?: number
  /** Display name of the assigned server. */
  serverName?: string
}

/** DTO for a cancellation request in the admin list. */
export interface CancellationRequestItem {
  /** Unique identifier. */
  id: number
  /** ID of the service being cancelled. */
  serviceId: number
  /** Name of the product/service. */
  serviceName: string
  /** ID of the owning client. */
  clientId: number
  /** Full name of the owning client. */
  clientName: string
  /**
   * Cancellation type as the backend enum member name -- `"Immediate"` or
   * `"EndOfBillingPeriod"`. Not display text; the screen owns the wording.
   */
  type: string
  /** Reason provided by the client. */
  reason: string | null
  /** Request status ("Open" or "Closed"). */
  status: string
  /** ISO 8601 creation date. */
  createdAt: string
}

/** Filter and paging of the service list. */
export interface ServiceFilter {
  /** Page, 1-based. */
  page: number
  /** Items per page. */
  pageSize: number
  /** Keep only this client's services. */
  clientId?: number | string
}

/** Body of a service save: the editable fields of the detail page. */
export interface UpdateServicePayload {
  /** Primary domain of the service. */
  domain: string | null
  /** Dedicated IP address. */
  dedicatedIp: string | null
  /** Hosting account username. */
  username: string | null
  /** Hosting account password. */
  password: string | null
  /** Reference on the provisioning server. */
  provisioningRef: string | null
  /** Recurring amount. */
  recurringAmount: number
  /** First payment amount. */
  firstPaymentAmount: number
  /** ISO 8601 next renewal date. */
  nextRenewalAt: string | null
  /** ISO 8601 creation date. */
  createdAt: string | null
  /** ISO 8601 termination date. */
  terminatedAt: string | null
  /** Billing cycle label. */
  billingCycle: string
  /** Payment method label. */
  paymentMethod: string | null
  /** Applied promotion code. */
  promotionCode: string | null
  /** External subscription reference. */
  subscriptionId: string | null
  /** Whether auto-suspend is overridden. */
  overrideAutoSuspend: boolean
  /** ISO 8601 date the override lasts until. */
  suspendUntil: string | null
  /** Whether the service terminates at the end of the cycle. */
  autoTerminateEndOfCycle: boolean
  /** Reason recorded for the automatic termination. */
  autoTerminateReason: string | null
  /** Free-text admin notes. */
  adminNotes: string | null
  /** Lifecycle status. */
  status: string
  /** Quantity. */
  quantity: number
  /** Server the service runs on. */
  serverId: number | null
  /** Product the service is of. */
  productId: number | null
}

/** Body of the suspend and terminate provisioning commands. */
export interface ProvisioningReasonPayload {
  /** Reason recorded with the command. */
  reason: string
}

/** Status of a hosted service, exactly as the API sends it. */
export enum ServiceStatus {
  Active = 'Active',
  Pending = 'Pending',
  Suspended = 'Suspended',
  Terminated = 'Terminated',
}

/** Closed set of billing cycles a service can renew on, as the API sends them. */
export const BillingCycle = {
  /** Never billed. */
  Free: 'free',
  /** Every month. */
  Monthly: 'monthly',
  /** Every three months. */
  Quarterly: 'quarterly',
  /** Every six months. */
  SemiAnnual: 'semi-annual',
  /** Every year. */
  Annual: 'annual',
  /** Every two years. */
  Biennial: 'biennial',
  /** Every three years. */
  Triennial: 'triennial',
} as const

/** One of the billing cycles. */
export type BillingCycle = (typeof BillingCycle)[keyof typeof BillingCycle]

/** How a cancellation request ends the service, exactly as the API sends it. */
export enum CancellationType {
  /** The service stops at once. */
  Immediate = 'Immediate',
  /** The service runs until the end of the paid period. */
  EndOfBillingPeriod = 'EndOfBillingPeriod',
}

/** Where a cancellation request stands, exactly as the API sends it. */
export enum CancellationStatus {
  /** Waiting for the operator. */
  Open = 'Open',
  /** Handled. */
  Closed = 'Closed',
}
