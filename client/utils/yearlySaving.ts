/**
 * The yearly saving of a plan, shared by every pricing surface that announces one.
 *
 * @module utils/yearlySaving
 */

import { MONTHS_PER_YEAR } from '~/constants/pricingCycle'
import type { PortalProductPricing } from '~/types/portalproductpricing'

/**
 * The saving of one per-month price over another, as a whole percent.
 *
 * @param fullPerMonth The per-month price paid month to month.
 * @param discountedPerMonth The per-month price after the saving (the annual price divided by twelve).
 * @returns The rounded percent when it is a real saving; null when either price is missing, the full
 * price is not positive or the other one is no cheaper, because none of those is a discount worth announcing.
 */
export const savingPercent = (fullPerMonth: number | null | undefined, discountedPerMonth: number | null | undefined): number | null => {
  if (fullPerMonth === null || fullPerMonth === undefined || discountedPerMonth === null || discountedPerMonth === undefined || fullPerMonth <= 0) {
    return null
  }
  const percent = Math.round((1 - discountedPerMonth / fullPerMonth) * 100)
  return percent > 0 ? percent : null
}

/**
 * The saving of paying a plan for a year at once instead of twelve times.
 *
 * @param pricing The plan's prices, or null/undefined when it has none.
 * @returns The rounded percent; null when the plan lacks either price or the annual one is no cheaper.
 */
export const yearlySavingPercent = (pricing: PortalProductPricing | null | undefined): number | null =>
  savingPercent(pricing?.monthly, pricing?.annual === null || pricing?.annual === undefined ? null : pricing.annual / MONTHS_PER_YEAR)
