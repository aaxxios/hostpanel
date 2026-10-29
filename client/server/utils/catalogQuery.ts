/**
 * The one reader of the public catalogue's query string.
 *
 * `GET /api/portal/public/products` is anonymous and its answers are cached under a key built from
 * the query, so every value in it is visitor text. This module turns the query into numbers and a
 * three-letter currency code or refuses the request: nothing else can reach the cache key or the
 * backend address, and a key can therefore only ever be short and of a fixed shape.
 *
 * @module server/utils/catalogQuery
 */

import type { H3Event } from 'h3'
import type { CatalogQuery } from '~/types/catalogquery'

/** The `data.code` of the 400 a malformed catalogue filter answers with. */
export const BAD_QUERY_PARAM = 'BAD_QUERY_PARAM'

/** The largest value of a 32-bit signed integer key. */
const MAX_INT_KEY = 2147483647

/** The most groups one request may name. */
export const MAX_GROUP_IDS = 20

/** A positive integer written without sign, fraction or leading zero. */
const POSITIVE_INTEGER = /^[1-9]\d{0,9}$/

/** An ISO 4217 alphabetic currency code. */
const CURRENCY_CODE = /^[A-Za-z]{3}$/

/**
 * The refusal every malformed filter answers with: a coded 400 and no sentence.
 *
 * @returns The H3 error to throw.
 */
const badQuery = () => createError({ statusCode: 400, data: { code: BAD_QUERY_PARAM } })

/**
 * Reads one query value as a single string.
 *
 * @param value - What `getQuery` returned for the name: absent, a string, or an array when the
 *   name was repeated.
 * @returns The string, or undefined when the name is absent or empty.
 * @throws H3Error 400 when the name was repeated.
 */
const singleValue = (value: unknown): string | undefined => {
  if (Array.isArray(value)) {
    throw badQuery()
  }
  return typeof value === 'string' && value !== '' ? value : undefined
}

/**
 * Parses one positive 32-bit integer.
 *
 * @param text - The text to parse.
 * @returns The number.
 * @throws H3Error 400 when the text is not a positive integer that fits a 32-bit key.
 */
const positiveInt = (text: string): number => {
  const parsed = Number(text)
  if (!POSITIVE_INTEGER.test(text) || parsed > MAX_INT_KEY) {
    throw badQuery()
  }
  return parsed
}

/**
 * Reads and validates the catalogue filters of a request.
 *
 * @param event - H3 event of the incoming browser request.
 * @returns The validated filters; `gids` is sorted and free of duplicates, so the same set of groups
 *   is always the same filter, and the currency is upper case.
 * @throws H3Error 400 carrying `data.code = BAD_QUERY_PARAM` when a filter is repeated, is not a
 *   positive integer (or three letters for the currency), or names more than {@link MAX_GROUP_IDS} groups.
 */
export const readCatalogQuery = (event: H3Event): CatalogQuery => {
  const query = getQuery(event)
  const pid = singleValue(query.pid)
  const gid = singleValue(query.gid)
  const gids = singleValue(query.gids)
  const currency = singleValue(query.currency)

  if (currency !== undefined && !CURRENCY_CODE.test(currency)) {
    throw badQuery()
  }

  const groupIds = gids === undefined
    ? undefined
    : [...new Set(gids.split(',').map(positiveInt))].sort((a, b) => a - b)
  if (groupIds && groupIds.length > MAX_GROUP_IDS) {
    throw badQuery()
  }

  return {
    pid: pid === undefined ? undefined : positiveInt(pid),
    gid: gid === undefined ? undefined : positiveInt(gid),
    gids: groupIds,
    currency: currency?.toUpperCase()
  }
}

/**
 * Builds the shared-cache key of a guest's catalogue answer.
 *
 * The key is made of the validated filters only: integers, a bounded sorted list and a three-letter
 * code, so it has a fixed shape whatever the visitor sent. The currency is part of it because a
 * response priced in one currency must never be served to a request that asked for another.
 *
 * @param query - The validated filters, from {@link readCatalogQuery}.
 * @returns The cache key.
 */
export const catalogCacheKey = (query: CatalogQuery): string => {
  const { pid, gid, gids, currency } = query
  return `products:p${pid ?? '-'}:g${gid ?? '-'}:gs${gids?.join(',') ?? '-'}:${currency ?? 'base'}`
}
