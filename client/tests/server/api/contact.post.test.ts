/**
 * Tests for `POST /api/contact`: the public contact form.
 *
 * The route's contract is that it never reports a success the backend did not earn: it answers
 * 204 only after the backend accepted the message, and a refusal keeps its status and code.
 *
 * @module tests/server/api/contact.post.test
 */

import { afterEach, beforeEach, describe, expect, it } from 'vitest'
import { makeEvent, refusal, startBackend } from '../../helpers/bffHarness'
import type { RouteModule } from '../../helpers/bffHarness'
import type { StubServer } from '../../helpers/stubServer'

/** The stub playing the C# API. */
let backend: StubServer

/** The route under test. */
let route: RouteModule

beforeEach(async () => {
  backend = await startBackend()
  route = await import('~/server/api/contact.post')
})

afterEach(async () => {
  await backend.close()
})

/** A complete form submission. */
const submission = {
  name: 'Ani',
  email: 'ani@example.test',
  phone: '+37400000000',
  service: 'hosting',
  message: 'Hello',
  timestamp: '2026-10-14T10:00:00Z',
}

describe('POST /api/contact', () => {
  it('relays the submission to the backend and answers 204 with no body', async () => {
    backend.respondWith(() => ({ status: 204, body: '' }))
    const event = makeEvent({ body: submission, headers: { 'x-language': 'ru' } })

    const result = await route.default(event)

    expect(result).toBeNull()
    expect(event.status).toBe(204)
    expect(backend.requests[0]?.url).toBe('/api/contact')
    expect(backend.requests[0]?.headers['x-language']).toBe('ru')
    expect(JSON.parse(backend.requests[0]?.body ?? '{}')).toEqual({
      name: 'Ani',
      email: 'ani@example.test',
      phone: '+37400000000',
      service: 'hosting',
      message: 'Hello',
      submittedAt: '2026-10-14T10:00:00Z',
    })
  })

  it('never reports success when the backend refuses: status, sentence, code and field errors survive', async () => {
    backend.respondWith(() => ({
      status: 400,
      body: { type: 'VALIDATION_ERROR', title: 'Invalid', detail: 'Fix the form.', errors: { email: ['Bad email.'] } },
    }))
    const event = makeEvent({ body: { ...submission, email: 'nope' } })

    const error = await refusal(() => route.default(event))

    expect(error).toMatchObject({
      statusCode: 400,
      statusMessage: 'Fix the form.',
      data: { code: 'VALIDATION_ERROR', errors: { email: ['Bad email.'] } },
    })
    expect(event.status).toBeUndefined()
  })

  it('does not report success when the backend is unreachable', async () => {
    await backend.close()

    const error = await refusal(() => route.default(makeEvent({ body: submission })))

    expect(error['statusCode']).toBe(502)
    backend = await startBackend()
  })
})
