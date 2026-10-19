/**
 * Whether a plan is free of charge.
 *
 * Decided by the price, never by the name, exactly as `stores/catalog.ts` does: matching "free"
 * in a name would treat a "Freelancer Hosting" that costs money as free and miss a free plan
 * called anything else.
 *
 * @module utils/isFreePlan
 */

import type { PortalProductPricing } from '~/types/portalproductpricing'

/**
 * Whether the plan's monthly price is zero.
 *
 * @param pricing - The plan's prices, or null/undefined when it has none.
 * @returns True only for an explicit zero monthly price; an absent price is not free.
 */
export const isFreePlan = (pricing: PortalProductPricing | null | undefined): boolean => pricing?.monthly === 0
