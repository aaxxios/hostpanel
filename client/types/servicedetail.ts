/**
 * One hosting service in full, as `GET /api/portal/client/services/{id}` returns it.
 *
 * @module types/servicedetail
 */

import type { ServiceConfigOption } from '~/types/serviceconfigoption'

/** One hosting service in full, as `GET /api/portal/client/services/{id}` returns it. */
export interface ServiceDetail {
  /** Service id. */
  id: number
  /** Product id the service was ordered from. */
  pid: number
  /** Registration date. */
  regdate: string
  /** Product name. */
  name: string
  /** Product group name. */
  groupname: string
  /** Primary domain. */
  domain: string
  /** Dedicated IP address, if any. */
  dedicatedip: string
  /** Server id the service is provisioned on. */
  serverid: number
  /** Server display name. */
  servername: string
  /** Server IP address. */
  serverip: string
  /** Server host name. */
  serverhostname: string
  /** Why the service is suspended; empty when it is not. */
  suspensionreason: string
  /** First payment amount. */
  firstpaymentamount: string
  /** Recurring amount. */
  recurringamount: string
  /** Payment method module. */
  paymentmethod: string
  /** Payment method display name. */
  paymentmethodname: string
  /** Billing cycle. */
  billingcycle: string
  /** Next due date. */
  nextduedate: string
  /** Lifecycle status, e.g. "Active" or "Pending". */
  status: string
  /** Control-panel account name; empty until set up. */
  username: string
  /** Disk used. */
  diskusage: string
  /** Disk quota. */
  disklimit: string
  /** Bandwidth used. */
  bwusage: string
  /** Bandwidth quota. */
  bwlimit: string
  /** When the usage figures were last refreshed. */
  lastupdate: string
  /** Configurable options chosen at order time. */
  configoptions?: {
    /** The chosen options. */
    configoption?: ServiceConfigOption[]
  }
}
