/**
 * A registered domain and everything that hangs off it — nameservers, DNS records,
 * forwarding rules and renewal reminders.
 */


/** Represents a domain registration. */
export interface Domain {
  /** Unique domain identifier. */
  id: number
  /** Associated client identifier. */
  clientId: number
  /** Domain name (e.g. example.com). */
  name: string
  /** Domain status (active, expired, pending). */
  status: string
  /** ISO 8601 expiry date. */
  expiresAt: string
}

/** DTO for domain registration list with client name and pricing. */
export interface DomainRegistration {
  /** Unique domain identifier. */
  id: number
  /** Associated client identifier. */
  clientId: number
  /** Full name of the owning client. */
  clientName: string
  /** Domain name (e.g. example.com). */
  name: string
  /** Registration period label (e.g. "1 Year"). */
  regPeriod: string
  /** Name of the registrar module. */
  registrar: string | null
  /** Recurring renewal amount. */
  recurringAmount: number
  /** ISO 4217 currency code for the price. */
  priceCurrency: string
  /** Next renewal payment due date (ISO 8601). */
  nextDueDate: string
  /** Domain expiration date (ISO 8601). */
  expiresAt: string
  /** Current lifecycle status. */
  status: string
  /** Whether auto-renew is enabled. */
  autoRenew: boolean
}

/** Full domain detail returned by GET /api/domains/{id}. */
export interface DomainDetail {
  /** Unique domain identifier. */
  id: number
  /** Associated client identifier. */
  clientId: number
  /** Fully-qualified domain name. */
  name: string
  /** Top-level domain suffix. */
  tld: string
  /** Current lifecycle status. */
  status: string
  /** ISO 8601 registration date. */
  registeredAt: string
  /** ISO 8601 expiry date. */
  expiresAt: string
  /** Whether auto-renew is enabled. */
  autoRenew: boolean
  /** Whether WHOIS privacy is enabled. */
  whoisPrivacy: boolean
  /** Whether registrar transfer lock is enabled. */
  isLocked: boolean
  /** Registrar-assigned reference. */
  registrarRef: string | null
  /** EPP authorization code. */
  eppCode: string | null
  /** Linked hosting service ID. */
  linkedServiceId: number | null
  /** One-time registration cost. */
  firstPaymentAmount: number
  /** Recurring renewal amount. */
  recurringAmount: number
  /** ISO 4217 currency code. */
  priceCurrency: string
  /** ISO 8601 next due date. */
  nextDueDate: string
  /** Registrar module name. */
  registrar: string | null
  /** Registration period in years. */
  registrationPeriod: number
  /** Payment method label. */
  paymentMethod: string | null
  /** Applied promotion code. */
  promotionCode: string | null
  /** External subscription reference. */
  subscriptionId: string | null
  /** Free-text admin notes. */
  adminNotes: string | null
  /** Order ID that created this domain. */
  orderId: number | null
  /** Order type: "Register" or "Transfer". */
  orderType: string
  /** Whether DNS management is enabled. */
  dnsManagement: boolean
  /** Whether email forwarding is enabled. */
  emailForwarding: boolean
  /** Nameserver list. */
  nameservers: NameserverItem[]
  /** DNS records. */
  dnsRecords: DnsRecordItem[]
  /** Email forwarding rules. */
  emailForwardingRules: EmailForwardingRuleItem[]
  /** Domain reminder history. */
  reminders: DomainReminderItem[]
}

/** Nameserver DTO. */
export interface NameserverItem {
  /** Nameserver primary key. */
  id: number
  /** Nameserver hostname. */
  host: string
}

/** DNS record DTO. */
export interface DnsRecordItem {
  /** Record primary key. */
  id: number
  /** Record type (A, AAAA, CNAME, MX, TXT, NS, SRV). */
  type: string
  /** Record host/name. */
  host: string
  /** Record value. */
  value: string
  /** Time-to-live in seconds. */
  ttl: number
  /** Priority (MX/SRV only). */
  priority: number | null
}

/** Email forwarding rule DTO. */
export interface EmailForwardingRuleItem {
  /** Rule primary key. */
  id: number
  /** Source alias. */
  source: string
  /** Destination email address. */
  destination: string
  /** Whether the rule is active. */
  isActive: boolean
}

/** Domain reminder history DTO. */
export interface DomainReminderItem {
  /** Reminder primary key. */
  id: number
  /** Reminder type label. */
  reminderType: string
  /** Recipient email. */
  sentTo: string
  /** ISO 8601 sent timestamp. */
  sentAt: string
}

/** Filter and paging of the domain list. */
export interface DomainFilter {
  /** Page, 1-based. */
  page: number
  /** Items per page. */
  pageSize: number
  /** Name search term; empty for none. */
  search?: string
  /** Keep only this client's domains. */
  clientId?: number | string
}

/** Body of a domain save: the editable fields of the detail page. */
export interface UpdateDomainPayload {
  /** One-time registration cost. */
  firstPaymentAmount: number
  /** Recurring renewal amount. */
  recurringAmount: number
  /** Payment method label. */
  paymentMethod: string | null
  /** Applied promotion code. */
  promotionCode: string | null
  /** External subscription reference. */
  subscriptionId: string | null
  /** Free-text admin notes. */
  adminNotes: string | null
  /** ISO 8601 expiry date. */
  expiresAt: string | null
  /** ISO 8601 next due date. */
  nextDueDate: string | null
  /** Registration period in years. */
  registrationPeriod: number
  /** Lifecycle status. */
  status: string
  /** Nameserver hostnames, empty ones dropped. */
  nameservers: string[]
}

/** WHOIS registrant contact fields, keyed by the field name the API expects. */
export type DomainContactPayload = Record<string, string>

/** Body that adds or replaces a DNS record. */
export interface DnsRecordPayload {
  /** Record type (A, MX, ...). */
  type: string
  /** Host label. */
  host: string
  /** Record value. */
  value: string
  /** Time-to-live in seconds. */
  ttl: number
  /** Priority (MX/SRV only), null otherwise. */
  priority: number | null
}

/** Body that adds or replaces an email forwarding rule. */
export interface ForwardingRulePayload {
  /** Source alias. */
  source: string
  /** Destination email address. */
  destination: string
  /** Whether the rule is active; omitted to keep the current state. */
  isActive?: boolean
}

/** A boolean switch of a domain feature (auto-renew, lock, privacy, DNS, forwarding). */
export interface DomainToggle {
  /** Whether the feature is turned on. */
  enabled: boolean
}

/** Status of a domain registration, exactly as the API sends it. */
export enum DomainStatus {
  Active = 'Active',
  PendingRegistration = 'PendingRegistration',
  PendingTransfer = 'PendingTransfer',
  Expired = 'Expired',
  Redemption = 'Redemption',
  Transferred = 'Transferred',
  Cancelled = 'Cancelled',
}

/** Closed set of DNS record types a domain's zone can hold. */
export const DnsRecordType = {
  A: 'A',
  Aaaa: 'AAAA',
  Cname: 'CNAME',
  Mx: 'MX',
  Txt: 'TXT',
  Ns: 'NS',
  Srv: 'SRV',
} as const

/** One of the DNS record types. */
export type DnsRecordType = (typeof DnsRecordType)[keyof typeof DnsRecordType]
