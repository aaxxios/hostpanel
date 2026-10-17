/**
 * Tests for the submission actions in `stores/support.ts`.
 *
 * The store, not the pages, owns the refusal sentence and the in-flight flag, so these
 * assertions pin the contract the pages render from.
 *
 * @module tests/stores/support.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { ref } from 'vue'

/** The mocked ticket read, re-created for every test. */
const fetchTicketMock = vi.fn()

/** The mocked department read, re-created for every test. */
const fetchDepartmentsMock = vi.fn()

/** The mocked new-ticket call, re-created for every test. */
const createTicketMock = vi.fn()

/** The mocked reply call, re-created for every test. */
const replyToTicketMock = vi.fn()

/** The mocked contact-form call, re-created for every test. */
const sendContactMessageMock = vi.fn()

vi.stubGlobal('ref', ref)

vi.mock('~/composables/apis/useSupportApi', () => ({
  /**
   * Stands in for the real API composable so the store runs without transport.
   *
   * @returns Only the endpoints the tests reach for.
   */
  useSupportApi: () => ({
    fetchTicket: fetchTicketMock,
    fetchDepartments: fetchDepartmentsMock,
    createTicket: createTicketMock,
    replyToTicket: replyToTicketMock,
    sendContactMessage: sendContactMessageMock
  })
}))

const { useSupportStore } = await import('~/stores/support')
const { SubmitOutcome } = await import('~/types/submitoutcome')
const { TicketPriority } = await import('~/types/ticketpriority')

/** A valid new-ticket body. */
const ticket = { departmentId: 1, subject: 's', message: 'm', priority: TicketPriority.Medium }

/** A valid contact-form body. */
const contact = { name: 'Ada', email: 'ada@example.com', message: 'hi' }

describe('useSupportStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    createTicketMock.mockReset()
    replyToTicketMock.mockReset()
    sendContactMessageMock.mockReset()
  })

  it('createTicket reports Sent and leaves no error', async () => {
    createTicketMock.mockResolvedValue({})
    const store = useSupportStore()

    expect(await store.createTicket(ticket)).toBe(SubmitOutcome.Sent)
    expect(store.ticketError).toBe('')
    expect(store.ticketLoading).toBe(false)
  })

  it('createTicket reports Failed with the API sentence', async () => {
    createTicketMock.mockRejectedValue({ data: { detail: 'Department closed.' } })
    const store = useSupportStore()

    expect(await store.createTicket(ticket)).toBe(SubmitOutcome.Failed)
    expect(store.ticketError).toBe('Department closed.')
    expect(store.ticketLoading).toBe(false)
  })

  it('replyToTicket reports Sent and Failed', async () => {
    const store = useSupportStore()

    replyToTicketMock.mockResolvedValue({})
    expect(await store.replyToTicket('7', 'hello')).toBe(SubmitOutcome.Sent)
    expect(replyToTicketMock).toHaveBeenCalledWith('7', 'hello')
    expect(store.replyError).toBe('')

    replyToTicketMock.mockRejectedValue({ data: { detail: 'Ticket closed.' } })
    expect(await store.replyToTicket('7', 'hello')).toBe(SubmitOutcome.Failed)
    expect(store.replyError).toBe('Ticket closed.')
    expect(store.replyLoading).toBe(false)
  })

  it('sendContactMessage reports Sent and Failed', async () => {
    const store = useSupportStore()

    sendContactMessageMock.mockResolvedValue(undefined)
    expect(await store.sendContactMessage(contact)).toBe(SubmitOutcome.Sent)
    expect(store.contactError).toBe('')

    // A rejection with no body is "no answer": the one client sentence, never `err.message`
    // (which would embed the upstream address).
    sendContactMessageMock.mockRejectedValue({ message: 'offline' })
    expect(await store.sendContactMessage(contact)).toBe(SubmitOutcome.Failed)
    expect(store.contactError).toBe('errors.noAnswer')
    expect(store.contactLoading).toBe(false)
  })

  it('keeps the field messages of a refused ticket and clears them on the next attempt', async () => {
    const store = useSupportStore()
    createTicketMock.mockRejectedValueOnce({
      data: { statusMessage: 'One or more validation errors occurred.', data: { code: 'VALIDATION_ERROR', errors: { subject: ['Subject is required.'] } } }
    })

    expect(await store.createTicket(ticket)).toBe(SubmitOutcome.Failed)
    expect(store.fieldErrors).toEqual({ subject: 'Subject is required.' })
    expect(store.ticketError).toBe('Subject is required.')

    createTicketMock.mockResolvedValueOnce({})
    expect(await store.createTicket(ticket)).toBe(SubmitOutcome.Sent)
    expect(store.fieldErrors).toEqual({})
  })

  it('keeps the field messages of a refused contact form', async () => {
    sendContactMessageMock.mockRejectedValue({ data: { type: 'VALIDATION_ERROR', errors: { email: ['Email is invalid.'] } } })
    const store = useSupportStore()

    expect(await store.sendContactMessage(contact)).toBe(SubmitOutcome.Failed)
    expect(store.fieldErrors).toEqual({ email: 'Email is invalid.' })
  })

  it('clears the previous error when a new attempt starts', async () => {
    const store = useSupportStore()
    createTicketMock.mockRejectedValueOnce({ message: 'boom' })
    await store.createTicket(ticket)
    createTicketMock.mockResolvedValueOnce({})
    await store.createTicket(ticket)

    expect(store.ticketError).toBe('')
  })
})

describe('useSupportStore reads', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    fetchTicketMock.mockReset()
    fetchDepartmentsMock.mockReset()
  })

  it('keeps the ticket with its thread', async () => {
    fetchTicketMock.mockResolvedValue({ subject: 'Help', status: 'Open' })
    const store = useSupportStore()

    expect(await store.loadTicket('8')).toMatchObject({ subject: 'Help' })
    expect(store.ticket).toMatchObject({ status: 'Open' })
    expect(store.ticketReading).toBe(false)
    expect(store.ticketReadError).toBe('')
  })

  it('keeps the API sentence and no ticket when the read is refused', async () => {
    fetchTicketMock.mockRejectedValue({ data: { detail: 'Not your ticket.' } })
    const store = useSupportStore()

    expect(await store.loadTicket('8')).toBeNull()
    expect(store.ticket).toBeNull()
    expect(store.ticketReadError).toBe('Not your ticket.')
  })

  it('keeps showing the ticket on screen while the same ticket is read again', async () => {
    fetchTicketMock.mockResolvedValueOnce({ subject: 'Help', replies: undefined })
    const store = useSupportStore()
    await store.loadTicket('8')

    let seenDuringRead: unknown = 'unset'
    fetchTicketMock.mockImplementationOnce(async () => {
      seenDuringRead = store.ticket
      return { subject: 'Help', replies: [] }
    })
    await store.loadTicket('8')

    expect(seenDuringRead).toMatchObject({ subject: 'Help' })
  })

  it('does not show the previous ticket while a different one is read', async () => {
    fetchTicketMock.mockResolvedValueOnce({ subject: 'First' })
    const store = useSupportStore()
    await store.loadTicket('8')

    let seenDuringRead: unknown = 'unset'
    fetchTicketMock.mockImplementationOnce(async () => {
      seenDuringRead = store.ticket
      return { subject: 'Second' }
    })
    await store.loadTicket('9')

    expect(seenDuringRead).toBeNull()
    expect(store.ticket).toMatchObject({ subject: 'Second' })
  })

  it('keeps the departments, and an empty list when they cannot be read', async () => {
    fetchDepartmentsMock.mockResolvedValueOnce([{ id: 1, name: 'Billing' }]).mockRejectedValueOnce(new Error('down'))
    const store = useSupportStore()

    await store.loadDepartments()
    expect(store.departments).toEqual([{ id: 1, name: 'Billing' }])

    await store.loadDepartments()
    expect(store.departments).toEqual([])
    expect(store.departmentsLoading).toBe(false)
  })
})
