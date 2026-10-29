/**
 * Tests for `server/utils/readQueryString.ts`: a single string passes, an absent key is undefined,
 * a repeated key (an array) is a coded 400.
 *
 * @module tests/server/utils/readQueryString.test
 */

import { describe, expect, it } from 'vitest'
import '../../helpers/bffHarness'
import { BAD_QUERY, readQueryString } from '~/server/utils/readQueryString'

describe('readQueryString', () => {
  it('returns a string as sent, empty included', () => {
    expect(readQueryString('a')).toBe('a')
    expect(readQueryString('')).toBe('')
  })

  it('returns undefined for an absent key', () => {
    expect(readQueryString(undefined)).toBeUndefined()
  })

  it.each([[['a', 'b']], [7], [null], [{ a: 1 }]])('refuses %j with a coded 400 and no sentence', (value) => {
    expect(() => readQueryString(value)).toThrow(expect.objectContaining({ statusCode: 400, data: { code: BAD_QUERY } }))
  })
})
