/**
 * Fixtures for the clients list and the client profile.
 */
import type { ClientDetail, ClientListItem } from '../../src/types/client'
import type { PagedResult } from '../../src/types/pagedresult'

/** The client the specs open. */
export const ada: ClientListItem = {
  id: 7,
  userId: 'user-7',
  email: 'ada@example.test',
  firstName: 'Ada',
  lastName: 'Lovelace',
  companyName: 'Analytical Engines',
  status: 'Active',
  isUserDeleted: false,
  twoFactorEnabled: true,
  createdAt: '2026-01-15T10:00:00Z',
}

/** A second row, so the list is seen to hold more than the one that gets clicked. */
export const grace: ClientListItem = {
  id: 8,
  userId: 'user-8',
  email: 'grace@example.test',
  firstName: 'Grace',
  lastName: 'Hopper',
  status: 'Active',
  isUserDeleted: false,
  twoFactorEnabled: false,
  createdAt: '2026-02-01T10:00:00Z',
}

/** `GET /api/clients` — one page holding both clients. */
export const clientsPage: PagedResult<ClientListItem> = {
  items: [ada, grace],
  totalCount: 2,
  page: 1,
  pageSize: 20,
}

/** `GET /api/clients/7` — the full record behind the profile form. */
export const adaDetail: ClientDetail = {
  ...ada,
  phone: '+1 5550100',
  street: '1 Difference Way',
  city: 'London',
  country: 'GB',
  currency: 'USD',
  notifyGeneral: true,
  notifyInvoice: true,
  notifySupport: true,
  notifyProduct: true,
  notifyDomain: true,
  notifyAffiliate: false,
  lateFees: false,
  overdueNotices: true,
  taxExempt: false,
  separateInvoices: false,
  disableCcProcessing: false,
  marketingOptIn: false,
  statusUpdate: false,
  allowSso: false,
  contacts: [],
}
