/**
 * Tests for the route-parameter guard in `server/utils/requireRouteParam.ts`.
 *
 * A missing or malformed parameter answers a coded 400 and carries no sentence: the BFF authors
 * none. A well-formed one is returned encoded.
 *
 * @module tests/server/utils/requireRouteParam.test
 */

import { describe, expect, it, vi } from 'vitest'
import type { H3Event } from 'h3'
import { RouteParamKind } from '~/types/routeparamkind'

/** Route parameters the fake event exposes. */
let params: Record<string, string | undefined> = {}

vi.stubGlobal('getRouterParam', (_event: H3Event, name: string) => params[name])
vi.stubGlobal('createError', (options: Record<string, unknown>) => Object.assign(new Error('h3'), options))

const { BAD_ROUTE_PARAM, requireRouteParam } = await import('~/server/utils/requireRouteParam')

/** An opaque event; the stubbed helper reads {@link params}. */
const event = {} as H3Event

/**
 * Runs the guard and returns what it threw.
 *
 * @param value - The parameter value the fake event carries.
 * @param kind - The kind to validate against; the guard's default when omitted.
 * @returns The thrown error, or undefined when the guard accepted the value.
 */
const refusalOf = (value: string | undefined, kind?: RouteParamKind): Record<string, unknown> | undefined => {
  params = { id: value }
  try {
    requireRouteParam(event, 'id', kind)
  } catch (error) {
    return error as Record<string, unknown>
  }
  return undefined
}

describe('requireRouteParam', () => {
  it('returns an integer parameter by default', () => {
    params = { id: '42' }

    expect(requireRouteParam(event, 'id')).toBe('42')
  })

  it.each([undefined, ''])('refuses %j with a coded 400 and no sentence', (value) => {
    const thrown = refusalOf(value)

    expect(thrown).toMatchObject({ statusCode: 400, data: { code: BAD_ROUTE_PARAM } })
    expect(thrown?.statusMessage).toBeUndefined()
  })

  it.each(['0', '-1', '01', '1.5', '7/../x', '7%2F', '7 ', 'abc', '2147483648', '99999999999', '7?x=1', '..'])(
    'refuses %j as an integer',
    (value) => {
      expect(refusalOf(value)).toMatchObject({ statusCode: 400, data: { code: BAD_ROUTE_PARAM } })
    }
  )

  it('accepts the largest 32-bit key', () => {
    params = { id: '2147483647' }

    expect(requireRouteParam(event, 'id', RouteParamKind.Int)).toBe('2147483647')
  })

  it('returns a GUID parameter', () => {
    params = { id: '3f2a1c4e-8b7d-4e6a-9c5b-1d2e3f4a5b6c' }

    expect(requireRouteParam(event, 'id', RouteParamKind.Guid)).toBe('3f2a1c4e-8b7d-4e6a-9c5b-1d2e3f4a5b6c')
  })

  it.each(['abc', '3f2a1c4e8b7d4e6a9c5b1d2e3f4a5b6c', '../y', '3f2a1c4e-8b7d-4e6a-9c5b-1d2e3f4a5b6c/x', '7'])(
    'refuses %j as a GUID',
    (value) => {
      expect(refusalOf(value, RouteParamKind.Guid)).toMatchObject({ statusCode: 400, data: { code: BAD_ROUTE_PARAM } })
    }
  )

  it.each(['7', 'pm_1Abc-Z', '3f2a1c4e-8b7d-4e6a-9c5b-1d2e3f4a5b6c'])('returns %j as an identifier', (value) => {
    params = { id: value }

    expect(requireRouteParam(event, 'id', RouteParamKind.Identifier)).toBe(value)
  })

  it.each(['a/b', '../y', 'a b', 'a?b', 'a#b', 'a\\b', 'a%2Fb', 'x'.repeat(129)])('refuses %j as an identifier', (value) => {
    expect(refusalOf(value, RouteParamKind.Identifier)).toMatchObject({ statusCode: 400, data: { code: BAD_ROUTE_PARAM } })
  })

  it('names the code the way the contract does', () => {
    expect(BAD_ROUTE_PARAM).toBe('BAD_ROUTE_PARAM')
  })
})
