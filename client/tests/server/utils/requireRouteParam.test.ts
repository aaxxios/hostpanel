/**
 * Tests for the route-parameter guard in `server/utils/requireRouteParam.ts`.
 *
 * A missing parameter answers a coded 400 and carries no sentence: the BFF authors none.
 *
 * @module tests/server/utils/requireRouteParam.test
 */

import { describe, expect, it, vi } from 'vitest'
import type { H3Event } from 'h3'

/** Route parameters the fake event exposes. */
let params: Record<string, string | undefined> = {}

vi.stubGlobal('getRouterParam', (_event: H3Event, name: string) => params[name])
vi.stubGlobal('createError', (options: Record<string, unknown>) => Object.assign(new Error('h3'), options))

const { BAD_ROUTE_PARAM, requireRouteParam } = await import('~/server/utils/requireRouteParam')

/** An opaque event; the stubbed helper reads {@link params}. */
const event = {} as H3Event

describe('requireRouteParam', () => {
  it('returns the parameter when it is present', () => {
    params = { id: '42' }

    expect(requireRouteParam(event, 'id')).toBe('42')
  })

  it.each([undefined, ''])('refuses %j with a coded 400 and no sentence', (value) => {
    params = { id: value }

    let thrown: Record<string, unknown> | undefined
    try {
      requireRouteParam(event, 'id')
    } catch (error) {
      thrown = error as Record<string, unknown>
    }

    expect(thrown).toMatchObject({ statusCode: 400, data: { code: BAD_ROUTE_PARAM } })
    expect(thrown?.statusMessage).toBeUndefined()
  })

  it('names the code the way the contract does', () => {
    expect(BAD_ROUTE_PARAM).toBe('BAD_ROUTE_PARAM')
  })
})
