/**
 * Tests for `GET /api/portal/client/services/:id`: it finds one service in the client's list and
 * maps it to the portal's service detail, answering 404 with a code when it is not theirs.
 *
 * @module tests/server/api/portal/client/services/[id]/index.get.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../../../../../helpers/bffHarness'
import type { RouteModule } from '../../../../../../helpers/bffHarness'
import type { StubServer } from '../../../../../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/portal/client/services/[id]/index.get')
})

afterEach(async () => {
  await backend.close()
})

describe('GET /api/portal/client/services/:id', () => {
  it('maps the matching service, including its server details', async () => {
    backend.respondWith(() => ({
      body: [
        { id: 3, productId: 1, productName: 'Other' },
        { id: 7, productId: 2, productName: 'Hosting M', serverId: 5, serverName: 'srv', serverIp: '1.2.3.4', serverHostname: 'srv.test', price: '5.00', paymentMethod: 'card', status: 'Active', createdAt: '2026-01-05T09:00:00Z' },
      ],
    }))

    const result = await route.default(makeEvent({ params: { id: '7' }, cookies: { auth_token: 'jwt' }, headers: { 'x-language': 'hy' } }))

    expect(backend.requests[0]?.url).toBe('/api/me/services')
    expect(backend.requests[0]?.headers['authorization']).toBe('Bearer jwt')
    expect(backend.requests[0]?.headers['x-language']).toBe('hy')
    expect(result).toMatchObject({
      id: 7,
      pid: 2,
      regdate: '2026-01-05T09:00:00Z',
      name: 'Hosting M',
      serverid: 5,
      servername: 'srv',
      serverip: '1.2.3.4',
      serverhostname: 'srv.test',
      recurringamount: '5.00',
      paymentmethod: 'card',
      paymentmethodname: 'card',
      username: '',
    })
  })

  it('refuses with 404 and the not-found code when the service is not in the list', async () => {
    backend.respondWith(() => ({ body: [{ id: 3 }] }))

    const error = await refusal(() => route.default(makeEvent({ params: { id: '7' }, cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 404, data: { code: 'SERVICE_NOT_FOUND' } })
  })

  it('passes the backend refusal through with its code and sentence', async () => {
    backend.respondWith(() => ({ status: 500, body: { type: 'INTERNAL_ERROR', detail: 'Failed.' } }))

    const error = await refusal(() => route.default(makeEvent({ params: { id: '7' }, cookies: { auth_token: 'jwt' } })))

    expect(error).toMatchObject({ statusCode: 500, statusMessage: 'Failed.', data: { code: 'INTERNAL_ERROR' } })
  })
})
