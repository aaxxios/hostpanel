/**
 * Fixtures for the support tickets list: two tickets on one page as `GET /api/tickets` returns them.
 */
import type { PagedResult } from '../../src/types/pagedresult'
import { TicketPriority, TicketStatus } from '../../src/types/ticket'
import type { TicketListItem } from '../../src/types/ticket'

/** An open ticket the operator can close. */
export const openListed: TicketListItem = {
  id: 42,
  subject: 'Site is down',
  status: TicketStatus.Open,
  priority: TicketPriority.High,
  createdAt: '2026-10-14T07:00:00Z',
  replyCount: 0,
  departmentName: 'Technical',
  lastReplyAt: null,
  isFlagged: false,
  clientId: 7,
}

/** A second ticket, so the list is seen to hold more than the one that gets selected. */
export const billingListed: TicketListItem = {
  ...openListed,
  id: 43,
  subject: 'Invoice question',
  departmentName: 'Billing',
  clientId: 8,
}

/** `GET /api/tickets` — one page holding both tickets. */
export const ticketsPage: PagedResult<TicketListItem> = {
  items: [openListed, billingListed],
  totalCount: 2,
  page: 1,
  pageSize: 20,
}
