/**
 * Tests for the Nitro error handler: an API error that carries `data` is answered as JSON with that
 * `data` and the hardening headers, and everything else is left unanswered so the next handler in
 * Nitro's chain (the one Nuxt registers, which renders `error.vue`) takes it.
 *
 * The handler runs inside a real `node:http` server with a real H3 event, and is read with a real
 * HTTP request. Only Nitro's own runtime module, which is not ours, is replaced.
 *
 * @module tests/server/error.test
 */

import { createServer, get } from 'node:http'
import type { Server } from 'node:http'
import type { AddressInfo } from 'node:net'
import { createError, createEvent } from 'h3'
import type { H3Error, H3Event } from 'h3'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

// Nitro's runtime is not available outside a Nitro build: the registration wrapper is the identity.
vi.mock('nitropack/runtime', () => ({ defineNitroErrorHandler: <T>(handler: T): T => handler }))

/** What a request to the test server came back with. */
interface Answer {
  /** HTTP status. */
  status: number
  /** Status line text. */
  statusMessage: string
  /** Response headers, names lower-cased by Node. */
  headers: Record<string, string | string[] | undefined>
  /** Response body. */
  body: string
}

/** The error handler's shape: it receives the error and the event. */
type ErrorHandler = (error: H3Error, event: H3Event) => Promise<unknown>

/** The server running the handler. */
let server: Server

/** The error the server's next request is answered with. */
let thrown: H3Error

/** The handler under test. */
let handler: ErrorHandler

/** Whatever the handler returned for the last request. */
let handled: unknown

/**
 * Requests a path from the test server.
 *
 * @param path - Request path and query.
 * @returns The status, headers and body of the answer.
 */
const request = (path: string): Promise<Answer> => {
  const { port } = server.address() as AddressInfo
  return new Promise((resolve, reject) => {
    get({ host: '127.0.0.1', port, path }, (response) => {
      const chunks: Buffer[] = []
      response.on('data', (chunk: Buffer) => {
        chunks.push(chunk)
      })
      response.on('end', () => {
        resolve({
          status: response.statusCode ?? 0,
          statusMessage: response.statusMessage ?? '',
          headers: response.headers,
          body: Buffer.concat(chunks).toString('utf8'),
        })
      })
    }).on('error', reject)
  })
}

beforeEach(async () => {
  handler = (await import('~/server/error')).default as unknown as ErrorHandler
  handled = undefined
  server = createServer((req, res) => {
    const event = createEvent(req, res)
    handler(thrown, event).then((result) => {
      handled = result
      if (!res.writableEnded) {
        res.end('next-handler')
      }
    }).catch(() => {
      res.statusCode = 500
      res.end()
    })
  })
  await new Promise<void>((resolve) => {
    server.listen(0, '127.0.0.1', resolve)
  })
})

afterEach(async () => {
  await new Promise<void>((resolve, reject) => {
    server.close(error => (error ? reject(error) : resolve()))
  })
})

describe('Nitro error handler', () => {
  it('answers an API error with its status, sentence and the data Nitro would drop', async () => {
    thrown = createError({ statusCode: 409, statusMessage: 'Refused.', data: { code: 'SOME_REFUSAL', title: 'Conflict', errors: { name: ['Taken.'] } } })

    const answer = await request('/api/portal/client/x?y=1')

    expect(answer.status).toBe(409)
    expect(answer.statusMessage).toBe('Refused.')
    expect(JSON.parse(answer.body)).toEqual({
      url: '/api/portal/client/x?y=1',
      statusCode: 409,
      statusMessage: 'Refused.',
      message: 'Refused.',
      data: { code: 'SOME_REFUSAL', title: 'Conflict', errors: { name: ['Taken.'] } },
    })
    expect(handled).toBeUndefined()
  })

  it('applies the same hardening headers as the built-in handler and answers JSON', async () => {
    thrown = createError({ statusCode: 400, statusMessage: 'Bad.', data: { code: 'BAD' } })

    const answer = await request('/api/x')

    expect(answer.headers['content-type']).toBe('application/json')
    expect(answer.headers['x-content-type-options']).toBe('nosniff')
    expect(answer.headers['x-frame-options']).toBe('DENY')
    expect(answer.headers['referrer-policy']).toBe('no-referrer')
    expect(answer.headers['content-security-policy']).toBe("script-src 'none'; frame-ancestors 'none';")
  })

  it('invents no sentence when the error carries a code only', async () => {
    thrown = createError({ statusCode: 404, data: { code: 'SERVICE_NOT_FOUND' } })

    const answer = await request('/api/x')

    expect(answer.status).toBe(404)
    expect(JSON.parse(answer.body)).toMatchObject({ statusCode: 404, statusMessage: '', data: { code: 'SERVICE_NOT_FOUND' } })
  })

  it('leaves an API error without data to the next handler', async () => {
    thrown = createError({ statusCode: 500, statusMessage: 'Boom.' })

    const answer = await request('/api/x')

    expect(handled).toBeUndefined()
    expect(answer.body).toBe('next-handler')
  })

  it('leaves a page error to the next handler, which renders error.vue', async () => {
    thrown = createError({ statusCode: 404, statusMessage: 'Page not found: /nope' })

    const answer = await request('/nope')

    expect(handled).toBeUndefined()
    expect(answer.body).toBe('next-handler')
  })

  it('leaves an error with data outside the API surface to the next handler', async () => {
    thrown = createError({ statusCode: 404, statusMessage: 'Missing.', data: { code: 'NOT_FOUND' } })

    const answer = await request('/client/dashboard')

    expect(handled).toBeUndefined()
    expect(answer.body).toBe('next-handler')
  })
})
