/**
 * Unit tests for the site type helpers: the type guessed from a slug and the cover initials.
 */
import { describe, expect, it } from 'vitest'
import { SiteType, initialsOf, siteTypeOf } from '~/utils/ui/siteType'

describe('siteTypeOf', () => {
  it.each([
    ['bluebird-shop', SiteType.Shop],
    ['northwind-portal', SiteType.Portal],
    ['harborline-tracking', SiteType.Tracking],
    ['fjordly-menu', SiteType.Restaurant],
    ['granite-library', SiteType.Docs],
    ['emberwood-magazine', SiteType.Publication],
    ['copperfield-booking', SiteType.Booking],
    ['meadowlark-donations', SiteType.Charity],
    ['sunpeak-tours', SiteType.Travel],
    ['bosphorus-portfolio', SiteType.Portfolio],
    ['something-else', SiteType.Website],
  ])('maps %s to %s', (slug, expected) => {
    expect(siteTypeOf(slug)).toBe(expected)
  })
})

describe('initialsOf', () => {
  it('takes up to two words, upper-cased', () => {
    expect(initialsOf('riverbend docs site')).toBe('RD')
  })

  it('returns an empty string for a blank title', () => {
    expect(initialsOf('   ')).toBe('')
  })
})
