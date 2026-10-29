/**
 * The address of the visitor behind a request, as far as it can be believed.
 *
 * `X-Forwarded-For` is a header the caller writes: every proxy appends the address it saw to the
 * right-hand end, so whatever a visitor puts there stays at the left-hand end. The leftmost entry
 * is therefore the one value an attacker controls, and reading it lets anyone mint a fresh rate-limit
 * bucket on every request. This module reads from the right instead, skipping exactly as many
 * entries as the deployment says it has trusted proxies in front of this server
 * (`runtimeConfig.trustedProxyCount`, `NUXT_TRUSTED_PROXY_COUNT`), the same reading as the API's
 * `RateLimit__TrustedProxyCount`. With none configured, the header is not read at all and the
 * socket address is the answer.
 *
 * @module server/utils/clientAddress
 */

import { isIP } from 'node:net'
import type { H3Event } from 'h3'

/** The header every proxy appends the address it received the request from to. */
const FORWARDED_FOR = 'x-forwarded-for'

/**
 * Reads how many trusted proxies stand in front of this server.
 *
 * @returns A whole number of proxies; zero when the setting is missing, negative or not a number.
 */
const trustedProxyCount = (): number => {
  const configured = Number(useRuntimeConfig().trustedProxyCount ?? 0)
  return Number.isInteger(configured) && configured > 0 ? configured : 0
}

/**
 * Resolves the visitor's address for a request.
 *
 * @param event - H3 event of the incoming browser request.
 * @returns With no trusted proxy, the socket address. With N trusted proxies, the Nth entry from the
 *   right of `X-Forwarded-For` (the leftmost when the header is shorter than N), provided it is an IP
 *   address; the socket address when the header is absent or that entry is not an address. Undefined
 *   only when the socket address is unknown too.
 */
export const resolveClientAddress = (event: H3Event): string | undefined => {
  const socketAddress = getRequestIP(event)
  const hops = trustedProxyCount()
  if (hops === 0) {
    return socketAddress
  }

  const entries = (getHeader(event, FORWARDED_FOR) ?? '')
    .split(',')
    .map(entry => entry.trim())
    .filter(entry => entry !== '')
  const entry = entries[Math.max(0, entries.length - hops)]

  return entry && isIP(entry) !== 0 ? entry : socketAddress
}
