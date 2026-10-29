/**
 * What the storefront needs to know about the two billing cycles the backend prices.
 *
 * The `PricingCycle` values are the keys of a product's `pricing`, but the locale files spell the
 * yearly cycle `annually`, so every page that names a cycle goes through this one map instead of
 * keeping its own copy.
 *
 * @module constants/pricingCycle
 */

import { BillingCycle } from '~/types/billingcycle'
import { PricingCycle } from '~/types/pricingcycle'

/** The cycles a plan can be sold in, in the order they are offered. */
export const PRICING_CYCLES: readonly PricingCycle[] = [PricingCycle.Monthly, PricingCycle.Annual]

/** Maps a cycle to the last segment of its `hosting.cycles.*` / `products.pricing.cycles.*` locale key. */
export const PRICING_CYCLE_I18N_KEY: Record<PricingCycle, string> = {
  [PricingCycle.Monthly]: 'monthly',
  [PricingCycle.Annual]: 'annually',
}

/** Maps the cycle of a service the client owns to the last segment of its `products.pricing.cycles.*` locale key. */
export const BILLING_CYCLE_I18N_KEY: Record<BillingCycle, string> = {
  [BillingCycle.Monthly]: PRICING_CYCLE_I18N_KEY[PricingCycle.Monthly],
  [BillingCycle.Annual]: PRICING_CYCLE_I18N_KEY[PricingCycle.Annual],
}

/** Months in a year, which turns an annual price into a per-month one. */
export const MONTHS_PER_YEAR = 12

/** Maps a cycle to the ISO 8601 duration one payment covers, for the structured data of an offer. */
export const PRICING_CYCLE_ISO_DURATION: Record<PricingCycle, string> = {
  [PricingCycle.Monthly]: 'P1M',
  [PricingCycle.Annual]: 'P1Y',
}

/** The billing cycle a domain registration is added to the cart with: domains are sold by the year. */
export const DOMAIN_BILLING_CYCLE = PricingCycle.Annual
