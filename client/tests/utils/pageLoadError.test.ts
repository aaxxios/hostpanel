/**
 * Tests for `utils/pageLoadError`: a detail page answers 404 only for a record the backend does
 * not have, and keeps the upstream status for any other failure.
 *
 * @module tests/utils/pageLoadError.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import { failureStatus, pageLoadError } from '~/utils/pageLoadError'

/** Stands in for the auto-imported `createError`, keeping the details it was given. */
const createError = vi.fn((details: object) => details)

describe('failureStatus', () => {
  it('reads the status under either name', () => {
    expect(failureStatus({ statusCode: 503 })).toBe(503)
    expect(failureStatus({ status: 500 })).toBe(500)
    expect(failureStatus({ statusCode: 404 })).toBe(404)
  })

  it('answers 502 when the request got no usable answer', () => {
    expect(failureStatus(new TypeError('fetch failed'))).toBe(502)
    expect(failureStatus(undefined)).toBe(502)
    expect(failureStatus('boom')).toBe(502)
    expect(failureStatus({ statusCode: 200 })).toBe(502)
  })
})

describe('pageLoadError', () => {
  beforeEach(() => {
    createError.mockClear()
    vi.stubGlobal('createError', createError)
  })

  it('is a 404 with the given message for a missing record', () => {
    expect(pageLoadError(404, 'Article not found')).toEqual({ statusCode: 404, statusMessage: 'Article not found' })
  })

  it('is a 404 when the read succeeded and returned nothing', () => {
    expect(pageLoadError(null, 'Article not found')).toMatchObject({ statusCode: 404 })
  })

  it('is a 404 when the id in the address is malformed', () => {
    expect(pageLoadError(400, 'Article not found')).toEqual({ statusCode: 404, statusMessage: 'Article not found' })
  })

  it.each([500, 502, 503])('keeps the upstream status %i and makes it fatal', (status) => {
    expect(pageLoadError(status, 'Article not found')).toMatchObject({ statusCode: status, fatal: true })
  })
})
