/**
 * Fixtures for the support ticket detail page.
 */
import type { Department } from '../../src/types/department'
import { TicketPriority, TicketStatus } from '../../src/types/ticket'
import type { Ticket } from '../../src/types/ticket'

/** `GET /api/tickets/departments`. */
export const departments: Department[] = [{ id: 1, name: 'Technical', email: 'tech@example.test' }]

/** `GET /api/tickets/42` before the staff reply. */
export const openTicket: Ticket = {
  id: 42,
  clientId: 7,
  subject: 'Site is down',
  message: 'My site returns an error since this morning.',
  status: TicketStatus.Open,
  priority: TicketPriority.High,
  departmentId: 1,
  departmentName: 'Technical',
  assignedToStaffId: null,
  assignedToStaffName: null,
  createdAt: '2026-10-14T07:00:00Z',
  replies: [],
}
