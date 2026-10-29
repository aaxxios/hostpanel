/**
 * Tests for `server/middleware/csrf.ts`: a state-changing request to this app's API must carry
 * `X-Requested-With: XMLHttpRequest`, with safe methods, non-API paths and bearer callers exempt.
 *
 * @module tests/server/middleware/csrf.test
 */

import { beforeAll, describe, expect, it } from 'vitest'
import { makeEvent, refusal } from '../../helpers/bffHarness'
import type { MiddlewareModule } from '../../helpers/bffHarness'
import { HttpMethod } from '~/types/httpmethod'

/** The middleware under test (its default export is a handler like a route's). */
let csrf: MiddlewareModule

beforeAll(async () => {
  csrf = await import('~/server/middleware/csrf')
})

describe('csrf middleware', () => {
  it.each([HttpMethod.Get, HttpMethod.Head, HttpMethod.Options])('lets the safe method %s through', async (method) => {
    await expect(Promise.resolve(csrf.default(makeEvent({ method, path: '/api/portal/client/me' })))).resolves.toBeUndefined()
  })

  it('lets a POST through when it carries the custom header', async () => {
    const event = makeEvent({ method: HttpMethod.Post, path: '/api/portal/auth/logout', headers: { 'x-requested-with': 'XMLHttpRequest' } })

    await expect(Promise.resolve(csrf.default(event))).resolves.toBeUndefined()
  })

  it('lets a bearer caller through without the header', async () => {
    const event = makeEvent({ method: HttpMethod.Post, path: '/api/portal/auth/logout', headers: { authorization: 'Bearer abc' } })

    await expect(Promise.resolve(csrf.default(event))).resolves.toBeUndefined()
  })

  it('does not touch page navigations', async () => {
    await expect(Promise.resolve(csrf.default(makeEvent({ method: HttpMethod.Post, path: '/client/login' })))).resolves.toBeUndefined()
  })

  it('refuses a cross-site style POST with 403', async () => {
    const error = await refusal(async () => csrf.default(makeEvent({ method: HttpMethod.Post, path: '/api/portal/auth/logout' })))

    expect(error['statusCode']).toBe(403)
  })
})
