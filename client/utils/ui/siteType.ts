/**
 * Site types and initials for the cover fallback of a project without a picture.
 *
 * @module utils/ui/siteType
 */
import { BookOpen, CalendarCheck, Globe, Heart, Newspaper, Palette, Plane, ShoppingBag, Truck, UserRound, Utensils } from 'lucide-vue-next'
import type { Component } from 'vue'

/** What a showcased site is, guessed from its slug. */
export enum SiteType {
  /** An online shop. */
  Shop = 'shop',
  /** A customer portal or account area. */
  Portal = 'portal',
  /** A shipment or order tracking dashboard. */
  Tracking = 'tracking',
  /** A restaurant or menu site. */
  Restaurant = 'restaurant',
  /** A documentation or document library site. */
  Docs = 'docs',
  /** A magazine, blog or news site. */
  Publication = 'publication',
  /** An appointment booking site. */
  Booking = 'booking',
  /** A charity or donations site. */
  Charity = 'charity',
  /** A travel or tours site. */
  Travel = 'travel',
  /** A portfolio of creative work. */
  Portfolio = 'portfolio',
  /** Anything else. */
  Website = 'website',
}

/** Slug fragments that mark each site type, checked in order. */
const SITE_TYPE_KEYWORDS: ReadonlyArray<readonly [SiteType, readonly string[]]> = [
  [SiteType.Shop, ['shop', 'store']],
  [SiteType.Portal, ['portal']],
  [SiteType.Tracking, ['tracking', 'track']],
  [SiteType.Restaurant, ['menu', 'restaurant']],
  [SiteType.Docs, ['docs', 'library']],
  [SiteType.Publication, ['magazine', 'blog', 'news']],
  [SiteType.Booking, ['booking']],
  [SiteType.Charity, ['donation', 'charity']],
  [SiteType.Travel, ['tour', 'travel']],
  [SiteType.Portfolio, ['portfolio']],
]

/** Icon drawn for each site type. */
export const SITE_TYPE_ICONS: Record<SiteType, Component> = {
  [SiteType.Shop]: ShoppingBag,
  [SiteType.Portal]: UserRound,
  [SiteType.Tracking]: Truck,
  [SiteType.Restaurant]: Utensils,
  [SiteType.Docs]: BookOpen,
  [SiteType.Publication]: Newspaper,
  [SiteType.Booking]: CalendarCheck,
  [SiteType.Charity]: Heart,
  [SiteType.Travel]: Plane,
  [SiteType.Portfolio]: Palette,
  [SiteType.Website]: Globe,
}

/**
 * Guesses what kind of site a project is from its slug.
 *
 * @param slug The project's URL-safe identifier.
 * @returns The first matching site type, or `Website`.
 */
export const siteTypeOf = (slug: string): SiteType => {
  const lower = slug.toLowerCase()
  for (const [type, keywords] of SITE_TYPE_KEYWORDS) {
    if (keywords.some((keyword) => lower.includes(keyword))) {
      return type
    }
  }
  return SiteType.Website
}

/**
 * Builds the initials shown on a cover: the first letters of up to two words of the title.
 *
 * @param title The project's title.
 * @returns One or two upper-case letters, or an empty string for a blank title.
 */
export const initialsOf = (title: string): string => {
  return title
    .split(/\s+/)
    .filter((word) => word.length > 0)
    .slice(0, 2)
    .map((word) => word.charAt(0).toUpperCase())
    .join('')
}
