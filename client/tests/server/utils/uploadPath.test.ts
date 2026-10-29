/**
 * Tests for the uploads path reader in `server/utils/uploadPath.ts`: the path is decoded once,
 * anything that could leave the uploads folder is a coded 400, and what is returned is the
 * re-encoded relative path.
 *
 * @module tests/server/utils/uploadPath.test
 */

import { describe, expect, it, vi } from 'vitest'
import type { H3Event } from 'h3'

/** Route parameters the fake event exposes. */
let params: Record<string, string | undefined> = {}

vi.stubGlobal('getRouterParam', (_event: H3Event, name: string) => params[name])
vi.stubGlobal('createError', (options: Record<string, unknown>) => Object.assign(new Error('h3'), options))

const { requireUploadPath } = await import('~/server/utils/uploadPath')

/** An opaque event; the stubbed helper reads {@link params}. */
const event = {} as H3Event

/**
 * Runs the reader for one raw `path` parameter.
 *
 * @param path - The raw (still percent-encoded) catch-all value.
 * @returns The accepted path.
 */
const read = (path: string | undefined): string => {
  params = { path }
  return requireUploadPath(event)
}

describe('requireUploadPath', () => {
  it.each([
    ['docs/a.pdf', 'docs/a.pdf'],
    ['a.png', 'a.png'],
    ['docs/my%20file.png', 'docs/my%20file.png'],
    ['docs/caf%C3%A9.png', 'docs/caf%C3%A9.png'],
    ['docs/plus+sign.png', 'docs/plus%2Bsign.png'],
    ['docs/...hidden', 'docs/...hidden']
  ])('accepts %s and returns the re-encoded path %s', (raw, expected) => {
    expect(read(raw)).toBe(expected)
  })

  it.each([
    undefined,
    '',
    '..',
    '.',
    '../a',
    'a/..',
    'a/../b',
    'a/./b',
    'a//b',
    '/a',
    'a/',
    '%2e%2e',
    '%2e%2e/a',
    '%2E%2E/a',
    '.%2e/a',
    '..%2fa',
    'a%2f..%2fb',
    'a%2f%2e%2e%2fb',
    'a\\b',
    'a%5cb',
    'a%5Cb',
    '%5c..%5ca',
    '%252e%252e/a',
    'a%252fb',
    '%25',
    'a%00b',
    'a%0ab',
    'a%7fb',
    '%E0%A4%A'
  ])('refuses %j with a coded 400 and no sentence', (raw) => {
    let thrown: Record<string, unknown> | undefined
    try {
      read(raw)
    } catch (error) {
      thrown = error as Record<string, unknown>
    }

    expect(thrown).toMatchObject({ statusCode: 400, data: { code: 'BAD_ROUTE_PARAM' } })
    expect(thrown?.statusMessage).toBeUndefined()
  })
})
