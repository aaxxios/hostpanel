/**
 * Tests for `composables/apis/useSupportApi.ts`.
 *
 * Each endpoint function is called for real through `apiFetch` against a stub server, and the
 * test asserts on the path, verb, query and body that arrived and on how the reply was typed.
 *
 * @module tests/composables/apis/useSupportApi.test
 */

import { afterEach, describe, expect, it } from 'vitest'
import { expectLastRequest, startTransport } from '../../helpers/stubTransport'
import type { ExpectedRequest } from '../../helpers/stubTransport'
import type { StubServer } from '../../helpers/stubServer'
import { useSupportApi } from '~/composables/apis/useSupportApi'

/** The stub started by the current test. */
let stub: StubServer | undefined

afterEach(async () => {
  await stub?.close()
  stub = undefined
})

/** The api surface under test. */
const api = useSupportApi()

/** Every endpoint function with the request it must send. */
const rows: ExpectedRequest[] = [
  { name: 'fetchTicket reads one ticket with its thread', call: () => api.fetchTicket('8'), method: 'GET', url: '/api/portal/client/tickets/8' },
  { name: 'fetchDepartments reads the public list', call: () => api.fetchDepartments(), method: 'GET', url: '/api/portal/public/departments' },
  { name: 'createTicket posts the ticket', call: () => api.createTicket({ departmentId: 1, subject: 's', message: 'm', priority: 'Medium' }), method: 'POST', url: '/api/portal/client/tickets', body: { departmentId: 1, subject: 's', message: 'm', priority: 'Medium' } },
  { name: 'replyToTicket posts the message only', call: () => api.replyToTicket('8', 'thanks'), method: 'POST', url: '/api/portal/client/tickets/8/reply', body: { message: 'thanks' } },
  { name: 'sendContactMessage posts to the site route', call: () => api.sendContactMessage({ name: 'A', email: 'a@b.c', message: 'hi' } as never), method: 'POST', url: '/api/contact', body: { name: 'A', email: 'a@b.c', message: 'hi' } },
]

describe('useSupportApi', () => {
  it.each(rows)('$name', async (row) => {
    stub = await startTransport()

    await row.call()

    expectLastRequest(stub, row)
  })

  it('rejects with the field errors of a refused ticket', async () => {
    stub = await startTransport(() => ({ status: 400, body: { type: 'VALIDATION_ERROR', errors: { subject: ['Required.'] } } }))

    await expect(api.createTicket({} as never)).rejects.toMatchObject({ data: { errors: { subject: ['Required.'] } } })
  })
})
