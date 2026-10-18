/**
 * What the storefront needs to know about the two billing cycles the backend prices.
 *
 * The `PricingCycle` values are the keys of a product's `pricing`, but the locale files spell the
 * yearly cycle `annually`, so every page that names a cycle goes through this one map instead of
 * keeping its own copy.
 *
 * @module constants/pricingCycle
 */

import { PricingCycle } from '~/types/pricingcycle'

/** The cycles a plan can be sold in, in the order they are offered. */
export const PRICING_CYCLES: readonly PricingCycle[] = [PricingCycle.Monthly, PricingCycle.Annual]

/** Maps a cycle to the last segment of its `hosting.cycles.*` / `products.pricing.cycles.*` locale key. */
export const PRICING_CYCLE_I18N_KEY: Record<PricingCycle, string> = {
  [PricingCycle.Monthly]: 'monthly',
  [PricingCycle.Annual]: 'annually',
}

/** The billing cycle a domain registration is added to the cart with: domains are sold by the year. */
export const DOMAIN_BILLING_CYCLE = 'annually'
