/**
 * Picks the product families the home page shows.
 *
 * @module utils/homeProductFamilies
 */

import { HOME_PRODUCT_FAMILY_LIMIT } from '~/constants/homeProducts'
import type { ProductFamily } from '~/types/productfamily'

/**
 * The families the home page shows: the featured ones first, then the rest in the order they
 * came in (the catalogue's own order, the static domains card leading), cut to `limit`.
 *
 * @param families Every family, in catalogue order.
 * @param limit How many to keep; defaults to {@link HOME_PRODUCT_FAMILY_LIMIT}.
 * @returns At most `limit` families; the input is not changed.
 */
export const topProductFamilies = (families: readonly ProductFamily[], limit: number = HOME_PRODUCT_FAMILY_LIMIT): ProductFamily[] => {
  const featured = families.filter((family) => family.featured === true)
  const others = families.filter((family) => family.featured !== true)
  return [...featured, ...others].slice(0, Math.max(0, limit))
}
