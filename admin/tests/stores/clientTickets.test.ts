/**
 * Tests for the client tickets store: loads fill the state, writes report the outcome as a value.
 */

import { describe, expect, it } from 'vitest'
import { useClientTicketsStore } from '@/stores/clientTickets'
import { TicketPriority, TicketStatus } from '@/types/ticket'
import { json, problem, stubFetch, stubOffline, callOf } from '../helpers/fetch'
import { installStoreHooks } from '../helpers/storeHooks'

describe('clientTicketStore', () => {
  installStoreHooks()

  it('fetchClientTickets fills the list and passes the search', async () => {
    const mock = stubFetch(json({ items: [{ id: 1 }], totalCount: 1 }))
    const store = useClientTicketsStore()

    await store.fetchClientTickets(7, 1, 20, 'refund')
    expect(store.tickets).toEqual([{ id: 1 }])
    expect(callOf(mock).url).toBe('/api/tickets/client/7?page=1&pageSize=20&search=refund')
  })

  it('fetchById sets the open ticket and clears it on refusal', async () => {
    const store = useClientTicketsStore()
    stubFetch(json({ id: 3 }))
    await store.fetchById(3)
    expect(store.currentTicket).toEqual({ id: 3 })

    stubFetch(problem('Ticket not found.', 404))
    await store.fetchById(4)
    expect(store.currentTicket).toBeNull()
    expect(store.error).toBe('Ticket not found.')
  })

  it('fetchStats and fetchDepartments fill their state and stay silent on failure', async () => {
    const store = useClientTicketsStore()
    stubFetch(json({ openedThisMonth: 2, openedLastMonth: 1, openedThisYear: 5, openedLastYear: 4 }))
    await store.fetchStats(7)
    expect(store.stats.openedThisMonth).toBe(2)

    stubFetch(json([{ id: 1, name: 'Sales' }]))
    await store.fetchDepartments()
    expect(store.departments).toEqual([{ id: 1, name: 'Sales' }])

    stubOffline()
    await store.fetchStats(7)
    await store.fetchDepartments()
    expect(store.error).toBeNull()
  })

  it('createTicket returns the id, or null with the sentence on refusal', async () => {
    const store = useClientTicketsStore()
    const payload = { clientId: 7, subject: 's', message: 'm', departmentId: 1, priority: TicketPriority.High }
    stubFetch(json(15))
    expect(await store.createTicket(payload)).toBe(15)

    stubFetch(problem('Department is closed.', 409))
    expect(await store.createTicket(payload)).toBeNull()
    expect(store.error).toBe('Department is closed.')
  })

  it('updateTicket, deleteTicket and replyToTicket report the outcome', async () => {
    const store = useClientTicketsStore()
    const mock = stubFetch(new Response(null, { status: 204 }))
    expect(await store.updateTicket(3, { status: TicketStatus.Closed })).toBe(true)
    expect(await store.deleteTicket(3)).toBe(true)
    expect(await store.replyToTicket(3, 'hello')).toBe(true)
    expect(callOf(mock, 2)).toMatchObject({ url: '/api/tickets/3/reply', body: { message: 'hello' } })

    stubFetch(problem('Ticket is closed.', 409))
    expect(await store.updateTicket(3, {})).toBe(false)
    expect(await store.deleteTicket(3)).toBe(false)
    expect(await store.replyToTicket(3, 'x')).toBe(false)
    expect(store.error).toBe('Ticket is closed.')
  })
})
