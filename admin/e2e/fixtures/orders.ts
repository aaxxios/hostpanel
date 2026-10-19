/**
 * Fixtures for the orders, services and domain lists: one row of each as the list endpoints
 * return it.
 */
import type { DomainRegistration } from '../../src/types/domain'
import type { OrderListItem } from '../../src/types/order'
import { OrderStatus } from '../../src/types/order'
import type { PagedResult } from '../../src/types/pagedresult'
import type { ServiceListItem } from '../../src/types/service'

/** An order waiting for the operator, so accept and cancel are offered. */
export const pendingOrder: OrderListItem = {
  id: 31,
  orderNumber: 'ORD-0031',
  clientId: 7,
  clientName: 'Ada Lovelace',
  status: OrderStatus.Pending,
  paymentMethod: 'PayPal',
  total: 12.5,
  invoiceId: null,
  itemCount: 2,
  createdAt: '2026-10-10T10:00:00Z',
}

/** `GET /api/orders` — one page with the pending order. */
export const ordersPage: PagedResult<OrderListItem> = { items: [pendingOrder], totalCount: 1, page: 1, pageSize: 20 }

/** A hosting service of the first client. */
export const hostingService: ServiceListItem = {
  id: 41,
  clientId: 7,
  clientName: 'Ada Lovelace',
  productName: 'Shared Hosting',
  domain: 'engines.example.test',
  price: 9.99,
  priceCurrency: 'USD',
  billingCycle: 'monthly',
  status: 'Active',
  nextDueDate: '2026-12-01T00:00:00Z',
}

/** `GET /api/services` — one page with the hosting service. */
export const servicesPage: PagedResult<ServiceListItem> = { items: [hostingService], totalCount: 1, page: 1, pageSize: 20 }

/** A registered domain of the second client. */
export const registeredDomain: DomainRegistration = {
  id: 51,
  clientId: 8,
  clientName: 'Grace Hopper',
  name: 'compiler.example.test',
  regPeriodYears: 1,
  registrar: null,
  recurringAmount: 12.99,
  priceCurrency: 'USD',
  nextDueDate: '2027-01-01T00:00:00Z',
  expiresAt: '2027-01-01T00:00:00Z',
  status: 'Active',
  autoRenew: true,
}

/** `GET /api/domains` — one page with the domain. */
export const domainsPage: PagedResult<DomainRegistration> = { items: [registeredDomain], totalCount: 1, page: 1, pageSize: 20 }
