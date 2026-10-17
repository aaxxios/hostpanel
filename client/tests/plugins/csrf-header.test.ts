/**
 * Tests for `plugins/csrf-header.ts`: every request to this app's own API carries
 * `X-Requested-With: XMLHttpRequest`, and a request to another origin is left alone.
 *
 * @module tests/plugins/csrf-header.test
 */

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

/** The options object the plugin hands to `$fetch.create`. */
interface CreatedOptions {
  /** The hook that runs before every request. */
  onRequest: (context: RequestContext) => void
}

/** The part of the ofetch hook context the plugin reads and writes. */
interface RequestContext {
  /** The URL or Request being sent. */
  request: string
  /** The request options; `headers` is replaced by the plugin on older ofetch. */
  options: { headers?: Headers | Record<string, string> }
}

/** The options captured from the plugin's `$fetch.create` call. */
let created: CreatedOptions | undefined

/** The `$fetch` the test started with, restored afterwards. */
const originalFetch = globalThis.$fetch

beforeEach(async () => {
  created = undefined
  vi.stubGlobal('defineNuxtPlugin', (setup: () => void) => setup)
  globalThis.$fetch = {
    create: (options: CreatedOptions) => {
      created = options
      return 'wrapped'
    },
  } as unknown as typeof globalThis.$fetch
  const plugin = (await import('~/plugins/csrf-header')).default as unknown as () => void
  plugin()
})

afterEach(() => {
  globalThis.$fetch = originalFetch
})

/**
 * Runs the captured hook for one request.
 *
 * @param request - The URL being requested.
 * @param headers - The headers object the options start with.
 * @returns The options after the hook ran.
 */
const send = (request: string, headers: Headers | Record<string, string>): RequestContext['options'] => {
  const options: RequestContext['options'] = { headers }
  created?.onRequest({ request, options })
  return options
}

describe('csrf-header plugin', () => {
  it('replaces the global $fetch with the wrapped one', () => {
    expect(created).toBeDefined()
    expect(globalThis.$fetch as unknown).toBe('wrapped')
  })

  it('adds the header to a relative request', () => {
    const options = send('/api/portal/client/me', new Headers())

    expect((options.headers as Headers).get('X-Requested-With')).toBe('XMLHttpRequest')
  })

  it('normalises a plain-object headers value into a Headers carrying the original entries', () => {
    const options = send('/api/portal/auth/login', { 'X-Language': 'ru' })

    expect(options.headers).toBeInstanceOf(Headers)
    expect((options.headers as Headers).get('X-Requested-With')).toBe('XMLHttpRequest')
    expect((options.headers as Headers).get('X-Language')).toBe('ru')
  })

  it.each(['https://other.example/api', 'http://localhost:5000/api/x'])('leaves the absolute URL %s untouched', (url) => {
    const headers = new Headers()

    send(url, headers)

    expect(headers.has('X-Requested-With')).toBe(false)
  })
})
