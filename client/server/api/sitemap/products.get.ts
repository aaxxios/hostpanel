/**
 * GET /api/sitemap/products
 *
 * The sitemap entries of the product family pages, built from the catalogue at request time so
 * the sitemap lists exactly the families the operator has (never a fixed list of slugs).
 * The sitemap module reads this route through `sitemap.sources` in `nuxt.config.ts`.
 *
 * A catalogue that cannot be read yields no entries rather than an error, so a backend outage
 * cannot take the whole sitemap down with it.
 *
 * @module server/api/sitemap/products.get
 */

import { HOSTING_GROUP_ID } from '~/constants/catalogGroup'
import { SitemapChangeFrequency } from '~/types/sitemapchangefrequency'
import type { PortalProductGroup } from '~/types/portalproductgroup'
import { familyKey, isFamilyGroup } from '~/utils/catalogGroups'

/** How often a family page is expected to change. */
const FAMILY_CHANGE_FREQUENCY = SitemapChangeFrequency.Weekly

/** Priority of a family page, between the listing (0.9) and the default (0.7). */
const FAMILY_PRIORITY = 0.8

/** Handler: one sitemap entry per product family; `[]` (after logging) when the catalogue cannot be read. */
export default defineEventHandler(async (event) => {
  try {
    const groups = await internalApiCall<PortalProductGroup[]>(event, '/public/product-groups')
    return groups
      // Hosting has its own page (`/hosting`), which the static entries already list.
      .filter((group) => isFamilyGroup(group) && group.id !== HOSTING_GROUP_ID)
      .map((group) => ({
        loc: `/products/${familyKey(group)}`,
        changefreq: FAMILY_CHANGE_FREQUENCY,
        priority: FAMILY_PRIORITY
      }))
  } catch (error) {
    // Logged, not rethrown: the sitemap stays up, but an outage must leave a trace for the operator.
    console.error('[sitemap] product families could not be read', error)
    return []
  }
})
