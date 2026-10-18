/**
 * Tests for `requireClientSession` in `server/utils/client-session.ts`: a valid cookie yields the
 * session, anything else is a bare 401 (the BFF writes no sentence).
 *
 * @module tests/server/utils/client-session.test
 */

import { describe, expect, it, vi } from 'vitest'
import type { H3Event } from 'h3'

/** The `client_session` cookie value the fake request carries; undefined means absent. */
let cookie: string | undefined

vi.stubGlobal('getCookie', () => cookie)
vi.stubGlobal('createError', (options: Record<string, unknown>) => Object.assign(new Error('h3'), options))

const { requireClientSession } = await import('~/server/utils/client-session')

/** An opaque event; the stubbed cookie reader ignores it. */
const event = {} as H3Event

/**
 * Runs the guard and returns what it threw.
 *
 * @returns The thrown error, or undefined when the guard passed.
 */
const refusal = (): Record<string, unknown> | undefined => {
  try {
    requireClientSession(event)
  } catch (error) {
    return error as Record<string, unknown>
  }
  return undefined
}

describe('requireClientSession', () => {
  it('returns the parsed session', () => {
    cookie = JSON.stringify({ id: 7, email: 'ada@example.com', name: 'Ada' })

    expect(requireClientSession(event)).toEqual({ id: 7, email: 'ada@example.com', name: 'Ada' })
  })

  it.each([
    ['absent', undefined],
    ['not JSON', 'not-json'],
    ['without an id', JSON.stringify({ email: 'ada@example.com' })],
  ])('answers 401 when the cookie is %s', (_label, value) => {
    cookie = value

    expect(refusal()).toMatchObject({ statusCode: 401 })
  })
})
