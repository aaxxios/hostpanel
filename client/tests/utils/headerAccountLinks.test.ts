/**
 * Unit tests for the header account links: the six client-area destinations, localised, in the
 * client navigation's order.
 */
import { describe, expect, it } from 'vitest'
import { buildAccountLinks } from '~/utils/headerAccountLinks'

describe('buildAccountLinks', () => {
  it('lists the client-area pages with their glyphs and localised routes', () => {
    const links = buildAccountLinks((key) => key, (path) => `/en${path}`)

    expect(links.map((link) => link.to)).toEqual([
      '/en/client/dashboard',
      '/en/client/services',
      '/en/client/domains',
      '/en/client/invoices',
      '/en/client/tickets',
      '/en/client/account',
    ])
    expect(links.every((link) => link.icon !== undefined)).toBe(true)
    expect(links[0]?.label).toBe('client.nav.dashboard')
  })
})
