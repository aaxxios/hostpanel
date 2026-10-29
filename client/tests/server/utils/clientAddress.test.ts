/**
 * Tests for the visitor-address reader in `server/utils/clientAddress.ts`.
 *
 * The property under test is the spoof resistance: `X-Forwarded-For` is read from the right, by the
 * configured number of trusted proxies, so an entry the visitor wrote at the left is never the
 * answer; with no proxy configured the header is not read at all.
 *
 * @module tests/server/utils/clientAddress.test
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'
import type { H3Event } from 'h3'

/** A fake request: the header the proxies built and the address of the connection itself. */
interface FakeRequest {
  /** The incoming `X-Forwarded-For` value, when the request has one. */
  forwardedFor?: string
  /** The socket address of the connection. */
  socketAddress?: string
}

/** The trusted proxy count the runtime config reports. */
let configuredCount: unknown = 0

vi.stubGlobal('getHeader', (event: FakeRequest, name: string) => (name.toLowerCase() === 'x-forwarded-for' ? event.forwardedFor : undefined))
vi.stubGlobal('getRequestIP', (event: FakeRequest) => event.socketAddress)
vi.stubGlobal('useRuntimeConfig', () => ({ trustedProxyCount: configuredCount }))

const { resolveClientAddress } = await import('~/server/utils/clientAddress')

/**
 * Resolves the address for a fake request.
 *
 * @param request - What the request carries.
 * @returns The address the reader settled on.
 */
const resolve = (request: FakeRequest): string | undefined => resolveClientAddress(request as unknown as H3Event)

beforeEach(() => {
  configuredCount = 0
})

describe('resolveClientAddress with no trusted proxy', () => {
  it('answers the socket address and ignores a spoofed X-Forwarded-For', () => {
    expect(resolve({ forwardedFor: '1.2.3.4', socketAddress: '198.51.100.20' })).toBe('198.51.100.20')
  })

  it('answers nothing when the socket address is unknown too', () => {
    expect(resolve({ forwardedFor: '1.2.3.4' })).toBeUndefined()
  })

  it.each([undefined, null, '', -1, 1.5, 'abc', NaN])('treats the setting %j as no trusted proxy', (value) => {
    configuredCount = value

    expect(resolve({ forwardedFor: '1.2.3.4', socketAddress: '198.51.100.20' })).toBe('198.51.100.20')
  })
})

describe('resolveClientAddress behind trusted proxies', () => {
  it('takes the entry the single trusted proxy appended, not the leftmost one the visitor wrote', () => {
    configuredCount = 1

    expect(resolve({ forwardedFor: '6.6.6.6, 203.0.113.7', socketAddress: '10.0.0.2' })).toBe('203.0.113.7')
  })

  it('is not moved by any number of spoofed entries on the left', () => {
    configuredCount = 1

    expect(resolve({ forwardedFor: '1.1.1.1, 2.2.2.2, 3.3.3.3, 203.0.113.7', socketAddress: '10.0.0.2' })).toBe('203.0.113.7')
  })

  it('skips one entry per trusted proxy', () => {
    configuredCount = 2

    expect(resolve({ forwardedFor: '6.6.6.6, 203.0.113.7, 10.0.0.9', socketAddress: '10.0.0.2' })).toBe('203.0.113.7')
  })

  it('accepts the setting as the string a NUXT_ environment override produces', () => {
    configuredCount = '1'

    expect(resolve({ forwardedFor: '6.6.6.6, 203.0.113.7', socketAddress: '10.0.0.2' })).toBe('203.0.113.7')
  })

  it('answers the leftmost entry when the header is shorter than the configured chain', () => {
    configuredCount = 3

    expect(resolve({ forwardedFor: '203.0.113.7, 10.0.0.9', socketAddress: '10.0.0.2' })).toBe('203.0.113.7')
  })

  it('answers the socket address when the header is missing or empty', () => {
    configuredCount = 1

    expect(resolve({ socketAddress: '10.0.0.2' })).toBe('10.0.0.2')
    expect(resolve({ forwardedFor: ' , ', socketAddress: '10.0.0.2' })).toBe('10.0.0.2')
  })

  it('answers the socket address when the selected entry is not an IP address', () => {
    configuredCount = 1

    expect(resolve({ forwardedFor: '203.0.113.7, <script>', socketAddress: '10.0.0.2' })).toBe('10.0.0.2')
  })

  it('reads an IPv6 entry', () => {
    configuredCount = 1

    expect(resolve({ forwardedFor: '6.6.6.6, 2001:db8::7', socketAddress: '10.0.0.2' })).toBe('2001:db8::7')
  })
})
