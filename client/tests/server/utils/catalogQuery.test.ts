/**
 * Tests for the catalogue query reader in `server/utils/catalogQuery.ts`: every filter is a number
 * or a three-letter code, the group list is canonical, and anything else is a coded 400.
 *
 * @module tests/server/utils/catalogQuery.test
 */

import { describe, expect, it, vi } from 'vitest'
import type { H3Event } from 'h3'

/** The query string the fake event carries. */
let query: Record<string, unknown> = {}

vi.stubGlobal('getQuery', () => query)
vi.stubGlobal('createError', (options: Record<string, unknown>) => Object.assign(new Error('h3'), options))

const { BAD_QUERY_PARAM, MAX_GROUP_IDS, catalogCacheKey, readCatalogQuery } = await import('~/server/utils/catalogQuery')

/** An opaque event; the stubbed helper reads {@link query}. */
const event = {} as H3Event

/**
 * Reads the filters for one query string.
 *
 * @param next - The query the request carries.
 * @returns The validated filters.
 */
const read = (next: Record<string, unknown>) => {
  query = next
  return readCatalogQuery(event)
}

describe('readCatalogQuery', () => {
  it('reads an empty query as no filter', () => {
    expect(read({})).toEqual({ pid: undefined, gid: undefined, gids: undefined, currency: undefined })
  })

  it('reads the integers, the sorted unique group list and the upper-case currency', () => {
    expect(read({ pid: '7', gid: '3', gids: '9,2,9,5', currency: 'amd' }))
      .toEqual({ pid: 7, gid: 3, gids: [2, 5, 9], currency: 'AMD' })
  })

  it('treats an empty value like an absent one', () => {
    expect(read({ pid: '', gid: '', gids: '', currency: '' }))
      .toEqual({ pid: undefined, gid: undefined, gids: undefined, currency: undefined })
  })

  it('accepts the largest 32-bit key and exactly the most groups allowed', () => {
    const groups = Array.from({ length: MAX_GROUP_IDS }, (_, index) => index + 1)

    expect(read({ pid: '2147483647', gids: groups.join(',') })).toMatchObject({ pid: 2147483647, gids: groups })
  })

  it.each([
    { pid: '0' },
    { pid: '-3' },
    { pid: '007' },
    { pid: '1e3' },
    { pid: '0x10' },
    { pid: ' 7' },
    { pid: '2147483648' },
    { pid: '99999999999999999999' },
    { gid: 'abc' },
    { gid: '1;drop' },
    { gids: '1,2,x' },
    { gids: ',' },
    { gids: '1,,2' },
    { currency: 'US' },
    { currency: 'USDD' },
    { currency: '€€€' },
    { pid: ['1', '2'] },
    { gids: ['1', '2'] },
    { currency: ['USD', 'AMD'] }
  ])('refuses %j with a coded 400 and no sentence', (bad) => {
    let thrown: Record<string, unknown> | undefined
    try {
      read(bad)
    } catch (error) {
      thrown = error as Record<string, unknown>
    }

    expect(thrown).toMatchObject({ statusCode: 400, data: { code: BAD_QUERY_PARAM } })
    expect(thrown?.statusMessage).toBeUndefined()
  })

  it('refuses more groups than a request may name', () => {
    const groups = Array.from({ length: MAX_GROUP_IDS + 1 }, (_, index) => index + 1)

    expect(() => read({ gids: groups.join(',') })).toThrow()
  })

  it('does not count a repeated group twice against the limit', () => {
    expect(read({ gids: Array.from({ length: MAX_GROUP_IDS * 2 }, () => '4').join(',') }).gids).toEqual([4])
  })
})

describe('catalogCacheKey', () => {
  it('has a fixed shape for an empty query', () => {
    expect(catalogCacheKey(read({}))).toBe('products:p-:g-:gs-:base')
  })

  it('carries every validated filter and the currency', () => {
    expect(catalogCacheKey(read({ pid: '7', gid: '3', gids: '9,2', currency: 'usd' }))).toBe('products:p7:g3:gs2,9:USD')
  })

  it('is the same key for the same set of groups however they were written', () => {
    expect(catalogCacheKey(read({ gids: '5,4,5' }))).toBe(catalogCacheKey(read({ gids: '4,5' })))
  })

  it('keeps one entry per currency, so a response priced in one is never served for another', () => {
    expect(catalogCacheKey(read({ currency: 'AMD' }))).not.toBe(catalogCacheKey(read({ currency: 'USD' })))
    expect(catalogCacheKey(read({ currency: 'AMD' }))).not.toBe(catalogCacheKey(read({})))
  })

  it('stays short and free of free text for the longest query a request may carry', () => {
    const groups = Array.from({ length: MAX_GROUP_IDS }, (_, index) => 2147483647 - index).join(',')

    expect(catalogCacheKey(read({ pid: '2147483647', gid: '2147483647', gids: groups, currency: 'EUR' })).length).toBeLessThan(300)
  })
})
